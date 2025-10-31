using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Models
{
    public class ContentImage
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public byte[] ImageData { get; set; } = [];
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public List<EmailContentImages> ParentContents { get; set; } = [];
    }

    // Join table for many to many relationship between Images and Content
    public class EmailContentImages
    {
        public int Id { get; set; }
        public ContentImage Image { get; set; } = default!;
        public Content EmailContent { get; set; } = default!;
    }
}
