using System;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MagicFramework.Helpers;

namespace Ref3.Controllers
{
    public class RoldepArevisController : ApiController
    {
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();

        private string getHtmlformultiselect(string label, string placeholder, string datatextfield, string datavaluefield, string url)
        {
            string template = "<div class=\"k-edit-label\"><label for=\"required\">{0}</label></div><div class=\"k-edit-field\"><select id=\"required\" multiple=\"multiple\" data-placeholder=\"{1}\"...\"></select></div>";
            template += "<script>";
            template += "$(\"#required\").kendoMultiSelect({{dataTextField: \"{2}\", dataValueField: \"{3}\", dataSource: {{transport: {{read: {{dataType: \"json\", url: \"{4}\"}} }} }}, height: 300}}); ";
            template += "var multiSelect = $(\"#required\").data(\"kendoMultiSelect\"), setValue = function(e) {{if (e.type != \"keypress\" || kendo.keys.ENTER == e.keyCode) {{multiSelect.dataSource.filter({{}}); multiSelect.value($(\"#required\").val().split(\",\")); }} }};";
            template += "</script>";
            template = string.Format(template, label, placeholder, datatextfield, datavaluefield, url);
            return template;
        }

        [HttpPost]
        public string gethtmlforarevis()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement el = (XmlElement)doc.AppendChild(doc.CreateElement("Select"));
            el.SetAttribute("id", "arevis");
            el.SetAttribute("multiple", "multiple");
            el.SetAttribute("data-placeholder", "Seleziona area di visibilità ...");

            var resdb = (from arevis in _context.US_AREVIS_area_visibility
                         select new { US_AREVIS_ID = arevis.US_AREVIS_ID, US_AREVIS_DESCRIPTION = arevis.US_AREVIS_DESCRIPTION }).ToArray();

            List<ResponseForMultiSelect> l = new List<ResponseForMultiSelect>();

            foreach (var r in resdb)
            {
                el.AppendChild(doc.CreateElement("option")).InnerText = r.US_AREVIS_DESCRIPTION;
            }

            Console.WriteLine(doc.OuterXml);
            return doc.OuterXml;
        }
        
        // <select id="required" multiple="multiple" data-placeholder="Select attendees...">
        //    <option>Steven White</option>
        //    <option>Nancy King</option>
        //    <option>Nancy Davolio</option>
        //    <option>Robert Davolio</option>
        //    <option>Michael Leverling</option>
        //    <option>Andrew Callahan</option>
        //    <option>Michael Suyama</option>
        //    <option selected>Anne King</option>
        //    <option>Laura Peacock</option>
        //    <option>Robert Fuller</option>
        //    <option>Janet White</option>
        //    <option>Nancy Leverling</option>
        //    <option>Robert Buchanan</option>
        //    <option>Margaret Buchanan</option>
        //    <option selected>Andrew Fuller</option>
        //    <option>Anne Davolio</option>
        //    <option>Andrew Suyama</option>
        //    <option>Nige Buchanan</option>
        //    <option>Laura Fuller</option>
        //</select>

        [HttpPost]
        public string GetMultiSelectArevis() {
            string div = getHtmlformultiselect("","Selezione area di visibilità","US_AREVIS_DESCRIPTION","US_AREVIS_ID",@"/api/RolDepArevis/GetValues");
            return div;
        }
        
        [HttpGet]
        public List<ResponseForMultiSelect> GetValues()
        {
            // select from the database, skipping and taking the correct amount
            var resdb = (from arevis in _context.US_AREVIS_area_visibility
                         select new { US_AREVIS_ID = arevis.US_AREVIS_ID, US_AREVIS_DESCRIPTION = arevis.US_AREVIS_DESCRIPTION }).ToArray();

            List<ResponseForMultiSelect> l = new List<ResponseForMultiSelect>();

            foreach (var r in resdb)
            {
                l.Add(new ResponseForMultiSelect(r.US_AREVIS_ID, r.US_AREVIS_DESCRIPTION));
            }

            return l;
        }

    }

     
}