export const CSS = `
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&family=Fraunces:ital,opsz,wght@0,9..144,300;0,9..144,600;1,9..144,300&display=swap');

*,*::before,*::after { box-sizing: border-box; margin: 0; padding: 0; }

/* ═══════════════════════════════════════════════════
   LIGHT THEME (default)
   ═══════════════════════════════════════════════════ */
.theme-light {
  --bg:         #f2f4f8;
  --surface:    #ffffff;
  --surface-2:  #f7f8fc;
  --border:     #e4e8f0;
  --border-mid: #d0d6e8;
  --text:       #0d1b36;
  --text-2:     #4a5578;
  --muted:      #8892aa;
  --topbar-bg:  #0d1b36;
  --topbar-text:#ffffff;
  --nav-bg:     #0d1b36;
  --nav-text:   rgba(255,255,255,.3);
  --nav-active: #e8a838;
  --card-bg:    #ffffff;
  --stat-bg:    linear-gradient(140deg, #0d1b36 0%, #1e3160 100%);
  --stat-text:  #e8a838;
  --stat-label: rgba(255,255,255,.45);
  --input-bg:   #f7f8fc;
  --input-focus:#ffffff;
  --btn-p-bg:   #0d1b36;
  --btn-p-text: #ffffff;
  --sheet-bg:   #ffffff;
  --sheet-drag: #d0d6e8;
  --fidbox-bg:  linear-gradient(140deg, #0d1b36 0%, #1e3160 100%);
  --echild-bg:  #f0f3fb;
  --echild-text:#2d4a8a;
  --accent:     #4f6ef7;
  --accent-dim: #eef1fe;
  --gold:       #e8a838;
  --gold-bg:    #fef6e4;
  --danger:     #e03e3e;
  --danger-bg:  #fff0f0;
  --success:    #18a06b;
  --success-bg: #edfaf4;
  --shadow-xs:  0 1px 2px rgba(13,27,54,.06);
  --shadow-sm:  0 1px 4px rgba(13,27,54,.08), 0 2px 12px rgba(13,27,54,.06);
  --shadow:     0 2px 8px rgba(13,27,54,.08), 0 8px 24px rgba(13,27,54,.07);
  --shadow-lg:  0 8px 24px rgba(13,27,54,.12), 0 24px 48px rgba(13,27,54,.10);
  --ov-bg:      rgba(13,27,54,.6);
}

/* ═══════════════════════════════════════════════════
   DARK THEME (charcoal)
   ═══════════════════════════════════════════════════ */
.theme-dark {
  --bg:         #111318;
  --surface:    #1c1e26;
  --surface-2:  #22252f;
  --border:     #2a2d38;
  --border-mid: #363a48;
  --text:       #edf0f7;
  --text-2:     #9aa0b8;
  --muted:      #5e6480;
  --topbar-bg:  #16181f;
  --topbar-text:#edf0f7;
  --nav-bg:     #16181f;
  --nav-text:   rgba(255,255,255,.25);
  --nav-active: #e8a838;
  --card-bg:    #1c1e26;
  --stat-bg:    linear-gradient(140deg, #22252f 0%, #2a2d38 100%);
  --stat-text:  #e8a838;
  --stat-label: rgba(237,240,247,.4);
  --input-bg:   #22252f;
  --input-focus:#2a2d38;
  --btn-p-bg:   #4f6ef7;
  --btn-p-text: #ffffff;
  --sheet-bg:   #1c1e26;
  --sheet-drag: #363a48;
  --fidbox-bg:  linear-gradient(140deg, #22252f 0%, #2a2d38 100%);
  --echild-bg:  #22252f;
  --echild-text:#9aa0b8;
  --accent:     #6b84f8;
  --accent-dim: #1e2340;
  --gold:       #e8a838;
  --gold-bg:    #2a2010;
  --danger:     #f06060;
  --danger-bg:  #2a1010;
  --success:    #2ecc8a;
  --success-bg: #0e2a1e;
  --shadow-xs:  0 1px 2px rgba(0,0,0,.3);
  --shadow-sm:  0 1px 4px rgba(0,0,0,.3), 0 2px 12px rgba(0,0,0,.2);
  --shadow:     0 2px 8px rgba(0,0,0,.3), 0 8px 24px rgba(0,0,0,.25);
  --shadow-lg:  0 8px 24px rgba(0,0,0,.4), 0 24px 48px rgba(0,0,0,.35);
  --ov-bg:      rgba(0,0,0,.7);
}

/* ═══════════════════════════════════════════════════
   NAVY THEME (deep ocean)
   ═══════════════════════════════════════════════════ */
.theme-navy {
  --bg:         #0a1628;
  --surface:    #0f2040;
  --surface-2:  #142850;
  --border:     #1e3460;
  --border-mid: #264080;
  --text:       #c8d8f0;
  --text-2:     #7a9acc;
  --muted:      #4a6a9a;
  --topbar-bg:  #081220;
  --topbar-text:#c8d8f0;
  --nav-bg:     #081220;
  --nav-text:   rgba(200,216,240,.25);
  --nav-active: #e8a838;
  --card-bg:    #0f2040;
  --stat-bg:    linear-gradient(140deg, #081220 0%, #0f2040 100%);
  --stat-text:  #e8a838;
  --stat-label: rgba(200,216,240,.45);
  --input-bg:   #142850;
  --input-focus:#1a3060;
  --btn-p-bg:   #2e5cc8;
  --btn-p-text: #ffffff;
  --sheet-bg:   #0f2040;
  --sheet-drag: #1e3460;
  --fidbox-bg:  linear-gradient(140deg, #081220 0%, #0f2040 100%);
  --echild-bg:  #142850;
  --echild-text:#7a9acc;
  --accent:     #4f8ef7;
  --accent-dim: #0e2040;
  --gold:       #e8a838;
  --gold-bg:    #1a1408;
  --danger:     #f07070;
  --danger-bg:  #200808;
  --success:    #40d090;
  --success-bg: #081a10;
  --shadow-xs:  0 1px 2px rgba(0,0,0,.4);
  --shadow-sm:  0 1px 4px rgba(0,0,0,.4), 0 2px 12px rgba(0,0,0,.3);
  --shadow:     0 2px 8px rgba(0,0,0,.4), 0 8px 24px rgba(0,0,0,.35);
  --shadow-lg:  0 8px 24px rgba(0,0,0,.5), 0 24px 48px rgba(0,0,0,.45);
  --ov-bg:      rgba(0,0,0,.75);
}

/* ═══════════════════════════════════════════════════
   SHARED CONSTANTS (not theme-dependent)
   ═══════════════════════════════════════════════════ */
:root {
  --school-dot: #4f6ef7;
  --med-dot:    #f04f5f;
  --act-dot:    #18a06b;
  --other-dot:  #e8a838;
  --r:  16px;
  --rs: 10px;
  --rm: 12px;
}

html, body {
  height: 100%;
  background: #081220;
  font-family: 'Outfit', sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
#root { height: 100%; display: flex; justify-content: center; align-items: stretch; }

.app {
  display: flex; flex-direction: column;
  height: 100%; width: 100%; max-width: 430px;
  background: var(--bg);
  color: var(--text);
  position: relative; overflow: hidden;
  transition: background .25s, color .25s;
}

/* ── THEME TOGGLE BUTTON ── */
.topbar-right { display: flex; align-items: center; gap: 10px; }
.theme-btn {
  display: flex; align-items: center; justify-content: center;
  width: 32px; height: 32px; border-radius: 50%;
  background: rgba(255,255,255,.1); border: 1.5px solid rgba(255,255,255,.15);
  cursor: pointer; transition: all .2s; font-size: 15px;
}
.theme-btn:hover { background: rgba(255,255,255,.2); transform: rotate(20deg); }

/* ── AUTH ── */
.ascreen { flex: 1; display: flex; flex-direction: column; overflow-y: auto; background: var(--bg); }
.ahero {
  background: var(--topbar-bg);
  padding: 56px 28px 52px;
  position: relative; overflow: hidden; flex-shrink: 0;
}
.ahero::before {
  content: ''; position: absolute; top: -80px; right: -80px;
  width: 260px; height: 260px; border-radius: 50%;
  background: radial-gradient(circle, rgba(79,110,247,.2) 0%, transparent 70%);
}
.ahero::after {
  content: ''; position: absolute; bottom: -50px; left: -40px;
  width: 180px; height: 180px; border-radius: 50%;
  background: radial-gradient(circle, rgba(232,168,56,.12) 0%, transparent 70%);
}
.alogo { font-family: 'Fraunces', serif; font-size: 34px; color: var(--topbar-text); font-weight: 300; letter-spacing: -.5px; position: relative; z-index: 1; }
.alogo em { font-style: italic; color: var(--gold); }
.atag { font-size: 14px; color: rgba(255,255,255,.4); margin-top: 8px; position: relative; z-index: 1; }
.abody { padding: 32px 24px 52px; }
.atabs {
  display: flex; background: var(--surface-2); border-radius: var(--rm);
  padding: 4px; margin-bottom: 28px;
  border: 1px solid var(--border);
}
.atab {
  flex: 1; padding: 10px; text-align: center; font-size: 14px; font-weight: 600;
  border-radius: 9px; cursor: pointer; transition: all .2s;
  color: var(--muted); border: none; background: none; font-family: 'Outfit', sans-serif;
}
.atab.on { background: var(--btn-p-bg); color: var(--btn-p-text); box-shadow: var(--shadow-sm); }
.atitle { font-family: 'Fraunces', serif; font-size: 26px; color: var(--text); margin-bottom: 4px; font-weight: 300; letter-spacing: -.3px; }
.asub { font-size: 14px; color: var(--muted); margin-bottom: 24px; }

/* ── SHELL ── */
.shell { display: flex; flex-direction: column; height: 100%; overflow: hidden; }
.topbar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 16px 20px 14px;
  background: var(--topbar-bg);
  flex-shrink: 0;
  transition: background .25s;
}
.tlogo { font-family: 'Fraunces', serif; font-size: 21px; color: var(--topbar-text); font-weight: 300; letter-spacing: -.3px; }
.tlogo em { font-style: italic; color: var(--gold); }
.tav {
  width: 34px; height: 34px; border-radius: 50%;
  background: rgba(255,255,255,.12); border: 1.5px solid rgba(255,255,255,.18);
  display: flex; align-items: center; justify-content: center;
  font-size: 12px; font-weight: 700; color: var(--topbar-text);
  cursor: pointer; transition: all .2s; letter-spacing: .5px;
}
.tav:hover { background: rgba(255,255,255,.2); border-color: var(--gold); }
.page { flex: 1; overflow-y: auto; padding: 20px 16px 96px; transition: background .25s; }
.nav {
  position: absolute; bottom: 0; left: 0; right: 0;
  display: flex; background: var(--nav-bg);
  padding: 8px 4px 22px; z-index: 10;
  border-top: 1px solid rgba(255,255,255,.06);
  transition: background .25s;
}
.ni {
  flex: 1; display: flex; flex-direction: column; align-items: center; gap: 5px;
  padding: 6px 4px; cursor: pointer; color: var(--nav-text);
  transition: color .2s; border: none; background: none; font-family: 'Outfit', sans-serif;
}
.ni.on { color: var(--nav-active); }
.ni span { font-size: 10px; font-weight: 600; letter-spacing: .6px; text-transform: uppercase; }

/* ── CARDS ── */
.card {
  background: var(--card-bg); border: 1px solid var(--border);
  border-radius: var(--r); padding: 18px; margin-bottom: 12px;
  box-shadow: var(--shadow-sm);
  transition: background .25s, border-color .25s;
}
.ch { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
.ct { font-family: 'Fraunces', serif; font-size: 17px; color: var(--text); font-weight: 600; letter-spacing: -.2px; }
.empty { text-align: center; padding: 24px 12px; color: var(--muted); font-size: 13.5px; line-height: 1.5; }

/* ── STATS ── */
.stats { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-bottom: 12px; }
.sc {
  background: var(--stat-bg);
  border-radius: var(--r); padding: 18px 16px;
  box-shadow: var(--shadow); position: relative; overflow: hidden;
}
.sc::after {
  content: ''; position: absolute; top: -20px; right: -20px;
  width: 80px; height: 80px; border-radius: 50%;
  background: rgba(255,255,255,.04);
}
.sv { font-family: 'Fraunces', serif; font-size: 34px; color: var(--stat-text); font-weight: 300; line-height: 1; letter-spacing: -1px; }
.sl { font-size: 11px; color: var(--stat-label); margin-top: 5px; font-weight: 600; letter-spacing: .5px; text-transform: uppercase; }

/* ── GREETING ── */
.greet { margin-bottom: 18px; padding: 4px 2px; }
.gname { font-family: 'Fraunces', serif; font-size: 28px; color: var(--text); line-height: 1.2; font-weight: 300; letter-spacing: -.4px; }
.gname em { font-style: italic; color: var(--accent); }
.gdate { font-size: 13px; color: var(--muted); margin-top: 5px; }

/* ── FORMS ── */
.f { margin-bottom: 16px; }
.f label { display: block; font-size: 11.5px; font-weight: 600; color: var(--text-2); margin-bottom: 6px; letter-spacing: .5px; text-transform: uppercase; }
.fw { position: relative; }
.f input, .f select {
  width: 100%; padding: 13px 16px;
  border: 1.5px solid var(--border); border-radius: var(--rm);
  font-size: 15px; font-family: 'Outfit', sans-serif;
  background: var(--input-bg); color: var(--text);
  outline: none; transition: border-color .18s, background .18s, box-shadow .18s;
  appearance: none; font-weight: 400;
}
.f input:focus, .f select:focus {
  border-color: var(--accent); background: var(--input-focus);
  box-shadow: 0 0 0 3px rgba(79,110,247,.12);
}
.f input::placeholder { color: var(--muted); }
.ficon {
  position: absolute; right: 13px; top: 50%; transform: translateY(-50%);
  cursor: pointer; color: var(--muted); background: none; border: none;
  padding: 4px; display: flex; align-items: center; opacity: .7; transition: opacity .15s;
}
.ficon:hover { opacity: 1; }
.frow { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }

/* ── BUTTONS ── */
.btn {
  display: flex; align-items: center; justify-content: center; gap: 6px;
  padding: 13px 18px; border-radius: var(--rm);
  font-size: 15px; font-weight: 600; font-family: 'Outfit', sans-serif;
  cursor: pointer; transition: all .18s; border: none; width: 100%; letter-spacing: .1px;
}
.btn-p { background: var(--btn-p-bg); color: var(--btn-p-text); }
.btn-p:hover { filter: brightness(1.12); }
.btn-p:active { transform: scale(.98); }
.btn-gold { background: var(--gold); color: #0d1b36; font-weight: 700; }
.btn-o { background: transparent; color: var(--text); border: 1.5px solid var(--border); }
.btn-o:hover { background: var(--surface-2); border-color: var(--border-mid); }
.btn-gh { background: transparent; color: var(--muted); width: auto; padding: 7px 10px; }
.btn-gh:hover { color: var(--text); background: var(--surface-2); border-radius: var(--rs); }
.btn-sm { padding: 7px 14px; font-size: 13px; width: auto; border-radius: var(--rs); }
.btn:disabled { opacity: .4; cursor: not-allowed; }
.err {
  background: var(--danger-bg); color: var(--danger);
  padding: 11px 14px; border-radius: var(--rm); font-size: 13.5px;
  margin-bottom: 16px; border: 1px solid color-mix(in srgb, var(--danger) 25%, transparent);
}

/* ── EVENTS ── */
.ei { display: flex; gap: 13px; padding: 13px 0; border-bottom: 1px solid var(--border); }
.ei:last-child { border-bottom: none; padding-bottom: 0; }
.ei:first-child { padding-top: 0; }
.dot { width: 8px; height: 8px; border-radius: 50%; margin-top: 6px; flex-shrink: 0; }
.dot.school   { background: var(--school-dot); box-shadow: 0 0 0 3px rgba(79,110,247,.18); }
.dot.medical  { background: var(--med-dot);    box-shadow: 0 0 0 3px rgba(240,79,95,.18); }
.dot.activity { background: var(--act-dot);    box-shadow: 0 0 0 3px rgba(24,160,107,.18); }
.dot.other    { background: var(--other-dot);  box-shadow: 0 0 0 3px rgba(232,168,56,.18); }
.einfo { flex: 1; min-width: 0; }
.etitle { font-size: 14px; font-weight: 600; color: var(--text); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; letter-spacing: -.1px; }
.emeta { font-size: 12px; color: var(--muted); margin-top: 3px; }
.echild {
  display: inline-flex; align-items: center; gap: 4px;
  background: var(--echild-bg); color: var(--echild-text);
  font-size: 11px; padding: 3px 9px; border-radius: 20px;
  margin-top: 6px; font-weight: 600;
}
.enotes { font-size: 12px; color: var(--muted); margin-top: 5px; font-style: italic; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.ecanc { opacity: .35; }
.ecanc .etitle { text-decoration: line-through; }
.bcx {
  display: inline-block; background: var(--danger-bg); color: var(--danger);
  font-size: 10px; font-weight: 700; padding: 2px 8px; border-radius: 20px;
  margin-left: 7px; vertical-align: middle; letter-spacing: .3px;
}

/* ── RSVP ── */
.rsvps { display: flex; gap: 6px; margin-top: 9px; }
.rb {
  flex: 1; padding: 7px 3px; border-radius: var(--rs); font-size: 11px; font-weight: 600;
  border: 1.5px solid var(--border); background: var(--surface-2);
  cursor: pointer; font-family: 'Outfit', sans-serif; transition: all .15s; color: var(--muted);
}
.rb:hover { border-color: var(--border-mid); color: var(--text-2); }
.rb.rA { background: var(--success-bg); border-color: var(--success); color: var(--success); }
.rb.rD { background: var(--danger-bg);  border-color: var(--danger);  color: var(--danger); }
.rb.rT { background: var(--gold-bg);    border-color: var(--gold);    color: var(--gold); }
.atts { display: flex; gap: 5px; margin-top: 7px; flex-wrap: wrap; }
.ac { display: inline-flex; align-items: center; gap: 3px; font-size: 11px; padding: 3px 10px; border-radius: 20px; font-weight: 600; }
.acA { background: var(--success-bg); color: var(--success); }
.acD { background: var(--danger-bg);  color: var(--danger); }
.acT { background: var(--gold-bg);    color: var(--gold); }

/* ── WEEK VIEW ── */
.wnav {
  display: flex; align-items: center; justify-content: space-between;
  margin-bottom: 16px; background: var(--card-bg);
  border: 1px solid var(--border); border-radius: var(--r);
  padding: 4px; box-shadow: var(--shadow-xs);
}
.wlbl { font-family: 'Fraunces', serif; font-size: 15px; color: var(--text); font-weight: 600; letter-spacing: -.2px; }
.wbtn {
  width: 36px; height: 36px; border-radius: var(--rs);
  border: none; background: transparent; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  color: var(--text-2); transition: background .15s;
}
.wbtn:hover { background: var(--surface-2); }
.drow { margin-bottom: 6px; }
.dlbl {
  font-size: 11px; font-weight: 700; color: var(--muted); text-transform: uppercase;
  letter-spacing: .7px; display: flex; align-items: center; gap: 10px; padding: 6px 0 4px;
}
.dlbl::after { content: ''; flex: 1; height: 1px; background: var(--border); }
.dtoday .dlbl { color: var(--accent); }
.dtoday .dlbl::after { background: var(--accent); opacity: .25; }
.dfree { font-size: 12.5px; color: var(--muted); padding: 7px 0 7px 18px; opacity: .6; font-style: italic; }

/* ── FAMILY ── */
.mi { display: flex; align-items: center; gap: 12px; padding: 12px 0; border-bottom: 1px solid var(--border); }
.mi:last-child { border-bottom: none; }
.av {
  width: 40px; height: 40px; border-radius: 50%;
  background: var(--btn-p-bg);
  display: flex; align-items: center; justify-content: center;
  font-size: 13px; color: var(--btn-p-text); font-weight: 700;
  flex-shrink: 0; letter-spacing: .5px;
}
.mn { font-size: 14px; font-weight: 600; color: var(--text); letter-spacing: -.1px; }
.me { font-size: 12px; color: var(--muted); margin-top: 1px; }
.ci { display: flex; align-items: center; justify-content: space-between; padding: 12px 0; border-bottom: 1px solid var(--border); }
.ci:last-child { border-bottom: none; }
.cn { font-size: 14px; font-weight: 600; color: var(--text); letter-spacing: -.1px; }
.cd { font-size: 12px; color: var(--muted); margin-top: 2px; }
.cage { font-size: 11px; color: var(--accent); font-weight: 700; background: var(--accent-dim); padding: 3px 10px; border-radius: 20px; }
.fidbox {
  background: var(--fidbox-bg);
  border-radius: var(--r); padding: 16px 18px;
  display: flex; align-items: center; justify-content: space-between;
  margin-bottom: 12px; box-shadow: var(--shadow);
}
.fidlbl { font-size: 10.5px; text-transform: uppercase; letter-spacing: .6px; color: rgba(255,255,255,.4); margin-bottom: 5px; font-weight: 600; }
.fidval { font-family: 'SF Mono', 'Fira Code', monospace; font-size: 13px; color: var(--gold); font-weight: 600; letter-spacing: .5px; }

/* ── BOTTOM SHEET ── */
.ov {
  position: fixed; inset: 0; background: var(--ov-bg); z-index: 200;
  display: flex; align-items: flex-end; justify-content: center;
  backdrop-filter: blur(4px); animation: fadeIn .2s ease;
}
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
.sh {
  background: var(--sheet-bg); border-radius: 24px 24px 0 0;
  width: 100%; max-width: 430px; max-height: 92vh; overflow-y: auto;
  padding: 0 22px 44px; box-shadow: var(--shadow-lg);
  animation: slideUp .25s cubic-bezier(.32,.72,0,1);
}
@keyframes slideUp { from { transform: translateY(100%); } to { transform: translateY(0); } }
.shdrag { width: 36px; height: 4px; background: var(--sheet-drag); border-radius: 4px; margin: 14px auto 22px; }
.shtitle { font-family: 'Fraunces', serif; font-size: 22px; color: var(--text); margin-bottom: 22px; font-weight: 300; letter-spacing: -.3px; }

/* ── CHIPS ── */
.chips { display: flex; flex-wrap: wrap; gap: 8px; }
.chip {
  padding: 7px 16px; border-radius: 20px; font-size: 13px; font-weight: 500;
  border: 1.5px solid var(--border); background: var(--surface-2);
  cursor: pointer; font-family: 'Outfit', sans-serif; color: var(--text-2); transition: all .15s;
}
.chip:hover { border-color: var(--border-mid); color: var(--text); }
.chip.on { background: var(--btn-p-bg); border-color: var(--btn-p-bg); color: var(--btn-p-text); font-weight: 600; }

/* ── SPINNER ── */
.spin { width: 18px; height: 18px; border: 2px solid rgba(255,255,255,.25); border-top-color: #fff; border-radius: 50%; animation: rot .65s linear infinite; }
.spind { border-color: rgba(255,255,255,.12); border-top-color: var(--text); }
@keyframes rot { to { transform: rotate(360deg); } }

/* ── TOAST ── */
.toast {
  position: fixed; bottom: 100px; left: 50%; transform: translateX(-50%);
  background: var(--btn-p-bg); color: var(--btn-p-text); padding: 10px 22px;
  border-radius: 22px; font-size: 13.5px; font-weight: 600;
  z-index: 300; pointer-events: none; box-shadow: var(--shadow-lg);
  animation: toastIn .2s cubic-bezier(.32,.72,0,1);
}
@keyframes toastIn {
  from { opacity: 0; transform: translateX(-50%) translateY(8px); }
  to   { opacity: 1; transform: translateX(-50%) translateY(0); }
}

/* ── MISC ── */
.page-title { font-family: 'Fraunces', serif; font-size: 26px; color: var(--text); margin-bottom: 18px; font-weight: 300; letter-spacing: -.4px; }
`;