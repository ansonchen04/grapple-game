public enum RopeState {
    Hidden,  // rope is hidden
    Shot,  // when you click the mouse - when you shoot the grapple out
    Hooked,  // when the hook hits something
    Retracting,  // when you right click - pulling yourself towards the hook
    Slack  // either you run out of line when casting or the rope hits something before the hook does
}