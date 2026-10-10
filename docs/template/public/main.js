export default {
  start: () => {
    // Keep inherited APIs available without burying each type's own documentation.
    for (const members of document.querySelectorAll('dl.inheritedMembers')) {
      const details = document.createElement('details');
      details.className = 'inherited-members';
      const summary = document.createElement('summary');
      summary.textContent = members.querySelector('dt').textContent;
      details.append(summary);
      members.before(details);
      details.append(members);
    }
  },
};
