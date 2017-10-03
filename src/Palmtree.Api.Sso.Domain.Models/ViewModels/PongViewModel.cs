namespace Palmtree.Api.Sso.Domain.Models.ViewModels
{
    using System;

    public class PongViewModel
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }

        public DateTime PongedAt { get; set; } = DateTime.Now;
    }
}
