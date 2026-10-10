(function(){
  const apiBase = '';
  const nickInput = document.getElementById('nick');
  const passInput = document.getElementById('password');
  const loginBtn = document.getElementById('loginBtn');
  const registerBtn = document.getElementById('registerBtn');
  const accountMsg = document.getElementById('accountMsg');
  const startBtn = document.getElementById('startBtn');
  const stopBtn = document.getElementById('stopBtn');
  const statusSpan = document.getElementById('status');
  const queuedSpan = document.getElementById('queued');

  let trackId = null;
  let queued = 0;

  // Map setup
  const map = L.map('map').setView([52.2297,21.0122], 13); // default Warsaw
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
  }).addTo(map);
  const poly = L.polyline([], { color: 'red' }).addTo(map);

  function setMsg(msg, isError){ accountMsg.textContent = msg; accountMsg.style.color = isError? 'red':'green'; }

  loginBtn.addEventListener('click', async ()=>{
    const nick = nickInput.value.trim();
    const pass = passInput.value || '';
    if(!nick) { setMsg('Enter nick', true); return; }
    try{
      const res = await fetch(`/api/account/login?nick=${encodeURIComponent(nick)}&password=${encodeURIComponent(pass)}`, { method: 'POST' });
      if(res.status===200){ const js = await res.json(); if(js.success || js.needsRegistration){ setMsg(js.success? 'Logged in':'No password set, register', false);} else setMsg('Unexpected response', true);} 
      else if(res.status===401) setMsg('Invalid password', true);
      else if(res.status===404) setMsg('User not found', true);
      else setMsg('Login failed', true);
    }catch(e){ setMsg('Error: '+e.message, true); }
  });

  registerBtn.addEventListener('click', async ()=>{
    const nick = nickInput.value.trim();
    const pass = passInput.value || '';
    if(!nick) { setMsg('Enter nick', true); return; }
    try{
      const body = { nick: nick, name: nick };
      const res = await fetch(`/api/account/register?password=${encodeURIComponent(pass)}`, { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify(body) });
      if(res.ok){ setMsg('Registered/updated', false); } else setMsg('Register failed', true);
    }catch(e){ setMsg('Error: '+e.message, true); }
  });

  const loadTracksBtn = document.getElementById('loadTracksBtn');
  const tracksNickInput = document.getElementById('tracksNick');
  const tracksSelect = document.getElementById('tracksSelect');
  const showLogsBtn = document.getElementById('showLogsBtn');

  loadTracksBtn.addEventListener('click', async ()=>{
    const nick = tracksNickInput.value.trim();
    if(!nick){ setMsg('Enter nick to load tracks', true); return; }
    try{
      const res = await fetch(`/api/tracker/tracks?nick=${encodeURIComponent(nick)}`);
      if(!res.ok){ setMsg('Failed to load tracks', true); return; }
      const arr = await res.json();
      tracksSelect.innerHTML = '';
      arr.forEach(t=>{
        const opt = document.createElement('option'); opt.value = t.id; opt.text = `${t.id} - ${t.trackStart}`; tracksSelect.appendChild(opt);
      });
      queued = arr.length; queuedSpan.textContent = 'Loaded tracks: '+queued;
      setMsg('Tracks loaded', false);
    }catch(e){ setMsg('Error: '+e.message, true); }
  });

  showLogsBtn.addEventListener('click', async ()=>{
    const sel = tracksSelect.value; if(!sel){ setMsg('Select a track', true); return; }
    try{
      const res = await fetch(`/api/tracker/tracks/${sel}/logs`);
      if(!res.ok){ setMsg('Failed to load logs', true); return; }
      const logs = await res.json();
      poly.setLatLngs([]);
      logs.forEach(l=>{ poly.addLatLng([l.geo_latitude, l.geo_longitude]); });
      if(logs.length) map.fitBounds(poly.getBounds());
      setMsg('Logs displayed: '+logs.length, false);
    }catch(e){ setMsg('Error: '+e.message, true); }
  });

})();
