namespace AutoLease.Domain.Entities;

public class OfferApplication
{
    public int Id { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public OfferApplicationStatus Status { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewNotes { get; private set; }

    // Foreign keys
    public int OfferId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string? ReviewedByUserId { get; private set; }

    // Navigation properties
    public virtual Offer Offer { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;
    public virtual User? ReviewedByUser { get; private set; }

    protected OfferApplication() { } // For EF Core

    public OfferApplication(int offerId, string userId, string notes = "")
    {
        ValidateOfferId(offerId);
        ValidateUserId(userId);
        ValidateNotes(notes);

        OfferId = offerId;
        UserId = userId;
        Notes = notes;
        Status = OfferApplicationStatus.Pending;
        ApplicationDate = DateTime.UtcNow;
    }

    public void Approve(string reviewedByUserId, string? reviewNotes = null)
    {
        ValidateUserId(reviewedByUserId);
        
        if (Status != OfferApplicationStatus.Pending)
            throw new InvalidOperationException("Only pending applications can be approved");

        Status = OfferApplicationStatus.Approved;
        ReviewedByUserId = reviewedByUserId;
        ReviewNotes = reviewNotes;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Reject(string reviewedByUserId, string? reviewNotes = null)
    {
        ValidateUserId(reviewedByUserId);
        
        if (Status != OfferApplicationStatus.Pending)
            throw new InvalidOperationException("Only pending applications can be rejected");

        Status = OfferApplicationStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        ReviewNotes = reviewNotes;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != OfferApplicationStatus.Pending)
            throw new InvalidOperationException("Only pending applications can be cancelled");

        Status = OfferApplicationStatus.Cancelled;
        ReviewedAt = DateTime.UtcNow;
    }

    private static void ValidateOfferId(int offerId)
    {
        if (offerId <= 0)
            throw new ArgumentException("Invalid offer ID", nameof(offerId));
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    private static void ValidateNotes(string notes)
    {
        if (notes != null && notes.Length > 500)
            throw new ArgumentException("Notes cannot exceed 500 characters", nameof(notes));
    }
}

public enum OfferApplicationStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}