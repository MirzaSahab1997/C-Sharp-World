﻿namespace IdentityVerificationService.Domain.Entities.Common
{
    public class Auditable<T> : PrimaryKey<T>
    {
        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string ModifiedBy { get; set; } = null;
    }
}
