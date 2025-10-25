// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Make Catalog dropdown title navigate to catalog on click
document.addEventListener('DOMContentLoaded', function () {
  var el = document.getElementById('catalogoDropdown');
  if (!el) return;
  el.addEventListener('click', function (e) {
    // If user uses modifier keys or middle click, let browser handle
    if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
    e.preventDefault();
    e.stopPropagation();
    var href = el.getAttribute('href');
    if (href) window.location.assign(href);
  });
});

// Inject a global mobile search form into main when navbar is collapsed
document.addEventListener('DOMContentLoaded', function () {
  try {
    var isMobile = window.matchMedia('(max-width: 576px)').matches;
    if (!isMobile) return;
    var main = document.querySelector('main[role="main"]');
    if (!main) return;
    // Avoid duplicates if a page already provides a main search
    if (main.querySelector('input[name="q"]')) return;

    var form = document.createElement('form');
    form.method = 'get';
    form.action = '/Camisetas/Index';
    form.className = 'row g-2 mb-3 mobile-site-search';

    var colInput = document.createElement('div');
    colInput.className = 'col-9';
    var input = document.createElement('input');
    input.type = 'text';
    input.name = 'q';
    input.placeholder = 'Buscar camisetas...';
    input.className = 'form-control';
    var qs = new URLSearchParams(window.location.search).get('q');
    if (qs) input.value = qs;
    colInput.appendChild(input);

    var colBtn = document.createElement('div');
    colBtn.className = 'col-3';
    var btn = document.createElement('button');
    btn.type = 'submit';
    btn.className = 'btn btn-primary w-100';
    btn.textContent = 'Buscar';
    colBtn.appendChild(btn);

    form.appendChild(colInput);
    form.appendChild(colBtn);
    // Sticky placement just under navbar
    var nav = document.querySelector('header .navbar');
    var top = nav ? nav.getBoundingClientRect().height : 56;
    form.style.position = 'sticky';
    form.style.top = top + 'px';
    form.style.zIndex = 1045;
    form.style.background = '#0f1625';
    form.style.borderBottom = '1px solid #2b3448';
    form.style.padding = '8px 0';
    form.style.marginBottom = '8px';

    main.insertBefore(form, main.firstChild);
  } catch (e) { /* no-op */ }
});
