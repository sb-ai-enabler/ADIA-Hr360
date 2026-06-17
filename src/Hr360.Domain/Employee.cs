using System;
using System.Collections.Generic;

namespace Hr360.Domain;

public partial class Employee
{
    public Guid Id { get; set; }

    public string EntraObjectId { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Department { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ReviewAssignment> ReviewAssignmentReviewees { get; set; } = new List<ReviewAssignment>();

    public virtual ICollection<ReviewAssignment> ReviewAssignmentReviewers { get; set; } = new List<ReviewAssignment>();
}
