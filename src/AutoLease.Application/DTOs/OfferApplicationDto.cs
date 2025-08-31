using AutoLease.Domain.Entities;

namespace AutoLease.Application.DTOs;

public class OfferApplicationDto
{
    public int Id { get; set; }
    public DateTime ApplicationDate { get; set; }
    public OfferApplicationStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    // Foreign keys
    public int OfferId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? ReviewedByUserId { get; set; }

    // Navigation properties
    public OfferDto? Offer { get; set; }
    public UserDto? User { get; set; }
    public UserDto? ReviewedByUser { get; set; }

    // Computed properties
    public string StatusName => Status.ToString();
    public string StatusBadgeClass => GetStatusBadgeClass();
    public bool CanBeReviewed => Status == OfferApplicationStatus.Pending;

    private string GetStatusBadgeClass()
    {
        return Status switch
        {
            OfferApplicationStatus.Pending => "badge bg-warning",
            OfferApplicationStatus.Approved => "badge bg-success",
            OfferApplicationStatus.Rejected => "badge bg-danger",
            OfferApplicationStatus.Cancelled => "badge bg-secondary",
            _ => "badge bg-light"
        };
    }
}