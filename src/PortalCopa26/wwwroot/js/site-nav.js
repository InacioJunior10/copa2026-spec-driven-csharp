// Toggle do menu mobile (mesmo comportamento do protótipo). O link ativo já é
// resolvido pelo próprio Blazor (<NavLink>), não precisa de JS para isso aqui.
function initPortalCopaNav() {
  const toggle = document.getElementById('navToggle');
  const mob = document.getElementById('navMobile');
  if (!toggle || !mob || toggle.dataset.navBound) {
    return;
  }
  toggle.dataset.navBound = 'true';

  toggle.addEventListener('click', () => mob.classList.toggle('open'));
  document.addEventListener('click', (e) => {
    if (!toggle.contains(e.target) && !mob.contains(e.target)) {
      mob.classList.remove('open');
    }
  });
}

document.addEventListener('DOMContentLoaded', initPortalCopaNav);
document.addEventListener('enhancedload', initPortalCopaNav);
