mergeInto(LibraryManager.library, {
  ExitWebGLGame: function () {
    // Replace only the game frame, leaving the itch.io page around it open.
    window.location.replace("about:blank");
  }
});
