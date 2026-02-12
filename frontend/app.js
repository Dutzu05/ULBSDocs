/* global google */

(function () {
  const STORAGE_KEY = "ulbs.user";

  function $(id) {
    return document.getElementById(id);
  }

  function setStatus(message) {
    const el = $("status");
    if (el) el.textContent = message;
  }

  function setDebug(obj) {
    // Intentionally no-op in the minimal UX.
    void obj;
  }

  function apiBase() {
    // Default: same origin as the page.
    // Optionally override with ?api=https://host:port
    const param = new URLSearchParams(window.location.search).get("api");
    if (!param) return "";
    return param.replace(/\/$/, "");
  }

  async function apiGet(path) {
    const res = await fetch(apiBase() + path, { headers: { "Accept": "application/json" } });
    const text = await res.text();
    let json = null;
    try { json = text ? JSON.parse(text) : null; } catch { /* ignore */ }

    if (!res.ok) {
      const errMsg = (json && (json.error || json.message)) ? (json.error || json.message) : text;
      throw new Error(errMsg || `Request failed: ${res.status}`);
    }

    return json;
  }

  async function apiPostJson(path, body) {
    const res = await fetch(apiBase() + path, {
      method: "POST",
      headers: { "Content-Type": "application/json", "Accept": "application/json" },
      body: JSON.stringify(body)
    });

    const text = await res.text();
    let json = null;
    try { json = text ? JSON.parse(text) : null; } catch { /* ignore */ }

    if (!res.ok) {
      const errMsg = (json && (json.error || json.message)) ? (json.error || json.message) : text;
      throw new Error(errMsg || `Request failed: ${res.status}`);
    }

    return json;
  }

  async function apiPostFile(path, file, expectedContentTypePrefix) {
    const form = new FormData();
    form.append("file", file, file.name);

    const res = await fetch(apiBase() + path, {
      method: "POST",
      body: form
    });

    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `Request failed: ${res.status}`);
    }

    const ct = (res.headers.get("content-type") || "").toLowerCase();
    if (expectedContentTypePrefix && !ct.startsWith(expectedContentTypePrefix)) {
      // Still return blob, but surface mismatch.
      setDebug({ warning: "Unexpected response content-type", contentType: ct });
    }

    const blob = await res.blob();
    const contentDisposition = res.headers.get("content-disposition") || "";
    const filenameMatch = /filename\*?=(?:UTF-8''|\")?([^\";]+)/i.exec(contentDisposition);
    const filename = filenameMatch ? decodeURIComponent(filenameMatch[1].replace(/\"/g, "").trim()) : null;

    return { blob, filename, contentType: ct };
  }

  function saveUser(user) {
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(user));
  }

  function loadUser() {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return null; }
  }

  function clearUser() {
    sessionStorage.removeItem(STORAGE_KEY);
  }

  function disableDownload(anchorId) {
    const a = $(anchorId);
    if (!a) return;
    a.href = "#";
    a.setAttribute("aria-disabled", "true");
    a.setAttribute("tabindex", "-1");
  }

  function setDownloadLink(anchorId, blob, filename) {
    const a = $(anchorId);
    if (!a) return;

    const url = URL.createObjectURL(blob);
    a.href = url;
    a.download = filename;
    a.setAttribute("aria-disabled", "false");
    a.removeAttribute("tabindex");
  }

  async function initLoginPage() {
    const existing = loadUser();
    if (existing?.email) {
      window.location.href = "./convert.html";
      return;
    }

    setStatus("Loading Google sign-in…");

    let clientId;
    try {
      const cfg = await apiGet("/auth/google/client-id");
      clientId = cfg?.clientId;
    } catch (e) {
      setStatus("Failed to load Google ClientId from API.");
      setDebug({ error: e.message, hint: "Make sure the API is running and /auth/google/client-id returns a value." });
      return;
    }

    if (!clientId) {
      setStatus("Google ClientId missing.");
      setDebug({ error: "ClientId was empty" });
      return;
    }

    function onCredential(response) {
      const idToken = response?.credential;
      if (!idToken) {
        setStatus("Google sign-in did not return a token.");
        return;
      }

      setStatus("Verifying token with API…");
      setDebug({ received: "id_token", length: idToken.length });

      apiPostJson("/auth/google", { idToken })
        .then((user) => {
          saveUser(user);
          setStatus(`Signed in as ${user.email || "(unknown)"}. Redirecting…`);
          setDebug(user);
          window.location.href = "./convert.html";
        })
        .catch((e) => {
          setStatus("Sign-in failed.");
          setDebug({ error: e.message });
        });
    }

    const buttonEl = $("gsi-button");
    if (!buttonEl) return;

    const start = Date.now();
    while (!window.google?.accounts?.id) {
      if (Date.now() - start > 8000) break;
      await new Promise((r) => setTimeout(r, 50));
    }

    if (!window.google?.accounts?.id) {
      setStatus("Google Identity Services failed to load.");
      setDebug({ hint: "Check network access to https://accounts.google.com/gsi/client" });
      return;
    }

    google.accounts.id.initialize({
      client_id: clientId,
      callback: onCredential
    });

    google.accounts.id.renderButton(buttonEl, {
      theme: "outline",
      size: "large",
      text: "signin_with",
      shape: "rectangular",
      width: 320
    });

    setStatus("Ready.");
  }

  async function initConvertPage() {
    const user = loadUser();
    if (!user?.email) {
      window.location.href = "./index.html";
      return;
    }

    const userEl = $("user");
    if (userEl) userEl.textContent = `Signed in as ${user.email}`;

    const logoutBtn = $("logout");
    if (logoutBtn) {
      logoutBtn.addEventListener("click", () => {
        clearUser();
        window.location.href = "./index.html";
      });
    }

    let docxResult = null;

    const convertDocBtn = $("convertDoc");
    const convertPdfBtn = $("convertPdf");

    if (convertDocBtn) {
      convertDocBtn.addEventListener("click", async () => {
        const fileInput = $("docFile");
        const file = fileInput?.files?.[0];

        disableDownload("downloadDocx");
        disableDownload("downloadPdf");
        if (convertPdfBtn) convertPdfBtn.disabled = true;

        if (!file) {
          setStatus("Select a .doc file first.");
          return;
        }

        if (!/\.doc$/i.test(file.name)) {
          setStatus("Only .doc files are supported for this step.");
          return;
        }

        setStatus("Converting DOC to DOCX…");
        setDebug({ file: file.name, size: file.size });

        try {
          const res = await apiPostFile("/api/doc-docx/convert", file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
          const filename = res.filename || file.name.replace(/\.doc$/i, ".docx");
          docxResult = { blob: res.blob, filename };

          setDownloadLink("downloadDocx", res.blob, filename);
          setStatus("DOCX ready.");
          setDebug({ docx: { filename, bytes: res.blob.size } });

          if (convertPdfBtn) convertPdfBtn.disabled = false;
        } catch (e) {
          setStatus("Conversion failed.");
          setDebug({ error: e.message });
        }
      });
    }

    if (convertPdfBtn) {
      convertPdfBtn.addEventListener("click", async () => {
        if (!docxResult?.blob) {
          setStatus("Convert to DOCX first.");
          return;
        }

        setStatus("Converting DOCX to PDF…");
        setDebug({ docx: { filename: docxResult.filename, bytes: docxResult.blob.size } });

        try {
          const docxFile = new File([docxResult.blob], docxResult.filename, {
            type: "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
          });

          const res = await apiPostFile("/api/docx-to-pdf/convert", docxFile, "application/pdf");
          const filename = res.filename || docxResult.filename.replace(/\.docx$/i, ".pdf");

          setDownloadLink("downloadPdf", res.blob, filename);
          setStatus("PDF ready.");
          setDebug({ pdf: { filename, bytes: res.blob.size } });
        } catch (e) {
          setStatus("PDF conversion failed.");
          setDebug({ error: e.message });
        }
      });
    }

    setStatus("Ready.");
  }

  window.ULBS_APP = {
    initLoginPage,
    initConvertPage
  };
})();
