"""Minimal inline-styled HTML email layout (email clients need inline CSS)."""


def render(title: str, body: str, button_label: str, button_url: str) -> str:
    return f"""\
<!doctype html>
<html>
  <body style="font-family:-apple-system,Segoe UI,Roboto,sans-serif;background:#f8fafc;padding:24px;margin:0;">
    <div style="max-width:480px;margin:0 auto;background:#fff;border:1px solid #e2e8f0;border-radius:12px;padding:32px;">
      <h1 style="color:#0f172a;font-size:20px;margin:0 0 12px;">{title}</h1>
      <p style="color:#475569;font-size:14px;line-height:1.5;margin:0;">{body}</p>
      <a href="{button_url}" style="display:inline-block;margin-top:20px;background:#2563eb;color:#fff;text-decoration:none;padding:10px 20px;border-radius:6px;font-size:14px;font-weight:500;">{button_label}</a>
      <p style="color:#94a3b8;font-size:12px;margin-top:24px;line-height:1.5;">
        If the button doesn't work, paste this link into your browser:<br>{button_url}
      </p>
    </div>
  </body>
</html>"""
