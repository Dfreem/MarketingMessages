using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.Components.Carousel;

public class CarouselItemModel
{
    public bool Active { get; set; }
    public string MarkupContent { get; set; } = default!;
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";

}
