using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CornerkickApp.Shared.Models
{
  public class ContentViewModel : LayoutModel
  {
    public int ID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Contents { get; set; }
    public byte[] Image { get; set; }
  }
}
