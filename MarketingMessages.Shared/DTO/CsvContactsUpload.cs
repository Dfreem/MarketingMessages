using MarketingMessages.Shared.DTO.V1;

using Microsoft.AspNetCore.Components.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Shared.DTO;

public class CsvContactsUpload
{
    public string[][] CsvRows { get; set; } = new string[][] { };
    public string[] CsvHeader { get; set; } = [];

    public string ListName { get; set; } = "";
    public Dictionary<string, CsvFields> FieldMap { get; set; } = [];
}


