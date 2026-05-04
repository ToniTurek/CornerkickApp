using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;

namespace CornerkickApp.Shared.Models
{
  public class DatabaseModel : LayoutModel
  {
    public List<string[]> ltFilesCk { get; set; }
    public string sFileCkSelected { get; set; }
    //public IFormFile fileDbImport { set; get; }

    public string[] sFileCkOnline { get; set; }
    public string sFileCkOnlineSelected { get; set; } // Online game
  }
}
