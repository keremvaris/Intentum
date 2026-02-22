(function () {
  // Sidebar toggler (if #sidebar-toggler / #dashboard-sidebar exist)
  document.addEventListener('click', function (e) {
    if (!e.target.closest('#sidebar-toggler')) return;
    var sidebar = document.getElementById('dashboard-sidebar');
    if (!sidebar) return;
    e.preventDefault();
    e.stopPropagation();
    sidebar.classList.toggle('collapsed');
    var isOpen = !sidebar.classList.contains('collapsed');
    sidebar.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
    var toggler = document.getElementById('sidebar-toggler');
    if (toggler) toggler.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
  }, true);

  // Close mobile nav when a menu link is clicked (accordion headers do not close)
  document.addEventListener('click', function (e) {
    if (!e.target.closest('.nav-scrollable a.nav-link')) return;
    var toggler = document.querySelector('.navbar-toggler');
    if (toggler) toggler.click();
  });
})();
