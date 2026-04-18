using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MovieCatalog.DTOs
{
    public class MovieDto
    {
        [ JsonPropertyName("id") ]
        public string Id { get; set; }
        
        [ JsonPropertyName("title") ]
        public string Title { get; set; }

        [ JsonPropertyName("description") ]
        public string Description { get; set; }

    }

}
