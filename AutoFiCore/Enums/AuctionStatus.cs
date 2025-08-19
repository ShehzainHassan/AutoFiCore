namespace AutoFiCore.Enums
{
    /// <summary>
    /// Represents the various stages or states of an auction lifecycle.
    /// </summary>
    public enum AuctionStatus
    {
        /// <summary>
        /// The auction is created and scheduled to start at a future time.
        /// </summary>
        Scheduled,

        /// <summary>
        /// The auction is visible for preview, but bidding has not yet started.
        /// </summary>
        PreviewMode,

        /// <summary>
        /// The auction is currently active and open for bidding.
        /// </summary>
        Active,

        /// <summary>
        /// The auction has ended and is no longer accepting bids.
        /// </summary>
        Ended,

        /// <summary>
        /// The auction was cancelled before completion.
        /// </summary>
        Cancelled
    }
}
