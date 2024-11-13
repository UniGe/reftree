using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.Http;
using System.Xml;
using System.Threading;
using MagicFramework.Helpers;

namespace Ref3.Controllers
{

    public class StoredParams
    {
        public string StoredName { get; set; }
        public XmlDocument Xmlinput { get; set; }

        public StoredParams(XmlDocument xml, string storedname)
        {
            StoredName = storedname; ;
            Xmlinput = xml;
        }
    }

    public class ResponseForMultiSelect
    {
        public string Textfield { get; set; }
        public int Valuefield { get; set; }

        public ResponseForMultiSelect(int valuefield, string textfield) 
        {
            Valuefield = valuefield;
            Textfield = textfield;
        }
    }

    public class ElaborazioniController : ApiController
    {
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
        private MagicFramework.Data.MagicDBDataContext magic = new MagicFramework.Data.MagicDBDataContext(DBConnectionManager.GetMagicConnection());

        [HttpPost]
        public string GetTabStrip(dynamic data)
        {
            HtmlGenericControl div = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.div, null, null, null, "tabstrip");
            HtmlGenericControl ul = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.ul, null, null, null, null);

            try
            {
                // vista per Tabstrip elements
                //var res = _context.JO_GET_UI_ELEMENTS(MagicFramework.Helpers.SessionHandler.IdUser).Where(x => x.US_AREVIS_ID == MagicFramework.Helpers.SessionHandler.UserVisibilityGroup).ToList();
                var res = _context.JO_GET_UI_ELEMENTS(MagicFramework.Helpers.SessionHandler.IdUser).ToList();

                var list = MagicFramework.UserVisibility.UserVisibiltyInfo.GetUserGroupVisibiltyChildrenSet();
                List<int> listarray = list.Split(',').Select(n => int.Parse(n)).ToList();

                // Vista dei job attivi
                var jobs = (from j in _context.JO_ACTIVE_JOBS.Where(x => listarray.Contains(x.US_AREVIS_ID))
                            select j).ToList();

                //produzione dei div per i tab 
                var tab = res.GroupBy(x => x.JO_AREJOB_ID);
                bool first = true;
                foreach (var t in tab)
                {
                    HtmlGenericControl li = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.li, null, "background-color: " + t.First().JO_AREJOB_COLOUR, t.First().JO_AREJOB_DESCRIPTION, null);
                    if (first)
                    {
                        li.Attributes.Add("class", "k-state-active");
                        first = false;
                    }
                    ul.Controls.Add(li);

                    // creazione dei div e span delle classi job (CLAJOB)
                    HtmlGenericControl div_l1 = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.div, "tab tab_" + ul.Controls.Count.ToString(), "background-color: " + t.First().JO_AREJOB_COLOUR, null, null);
                    var l1 = t.GroupBy(x => x.JO_CLAJOB_ID);
                    foreach (var r in l1)
                    {
                        HtmlGenericControl div_l2 = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.div, "subarea", null, null, null);
                        HtmlGenericControl span = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.span, "spans", "background-color: " + t.First().JO_AREJOB_COLOUR, r.First().JO_CLAJOB_DESCRIPTION, null);
                        div_l2.Controls.Add(span);

                        // creazione dei pulsanti dei tipi job (TIPJOB)
                        //var l2 = r.Where(y => y.JO_TIPJOB_JO_CLAJOB_ID == r.First().JO_CLAJOB_ID).ToList();
                        foreach (var z in r)
                        {
                            int jobscount = jobs.Where(x => x.JO_JOBEVE_JO_TIPJOB_ID == z.JO_TIPJOB_ID).Count();
                            string label = z.JO_TIPJOB_DESCRIPTION + " (" + jobscount.ToString() + ")";
                            HtmlGenericControl a = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.a, "k-button buttons", null, label, null);
                            a.Attributes.Add("onclick", "refreshgrid(event," + z.JO_TIPJOB_ID + ",'" + z.US_USEPRO_EXEC_RIGHTS + "|" + z.US_USEPRO_UPDATE_RIGHTS + "|" + z.US_USEPRO_DELETE_RIGHTS + "|" + z.US_USEPRO_EXPORT_RIGHTS + "')");
                            div_l2.Controls.Add(a);
                        }
                        div_l1.Controls.Add(div_l2);
                    }
                    div.Controls.Add(div_l1);
                }
                div.Controls.AddAt(0, ul);

                //response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                //response.StatusCode = HttpStatusCode.InternalServerError;
                //response.Content = new StringContent(string.Format("Failed with message -{0}", ex.Message));                                                
                return (string.Format("Failed with message -{0}", ex.Message));
            }
            return HtmlControlsBuilder.HtmlControlToText(div);
        }

        private string getHtmlfordatarole(string datarole, List<MagicFramework.Data.Magic_TemplateDataRoles> templates)
        {
            var builder = new TemplateContainerBuilder(true);
            builder.PrepareDataRoles();
            return builder.GetTemplateContent(templates.Where(x => x.MagicTemplateDataRole == datarole).First().MagicTemplateDataRole);
        }

        [HttpGet]
        public List<ResponseForMultiSelect> GetGroups()
        {
            // select from the database, skipping and taking the correct amount
            var resdb = (from aregro in _context.US_AREGRO_area_groups
                         where aregro.US_AREGRO_US_AREVIS_ID == MagicFramework.Helpers.SessionHandler.UserVisibilityGroup
                         join groups in _context.US_GROUPS_groups on aregro.US_AREGRO_US_GROUPS_ID equals groups.US_GROUPS_ID
                         select new { US_GROUPS_ID = groups.US_GROUPS_ID, US_GROUPS_DESCRIPTION = groups.US_GROUPS_DESCRIPTION }).ToArray();

            List<ResponseForMultiSelect> l = new List<ResponseForMultiSelect>();

            foreach (var r in resdb)
            {
                l.Add(new ResponseForMultiSelect(r.US_GROUPS_ID, r.US_GROUPS_DESCRIPTION));
            }

            return l;
        }

        //private string getHtmlformultiselect(string label, string placeholder, string datatextfield, string datavaluefield, string url)
        //{
        //    string template = "<div class=\"k-edit-label\"><label for=\"required\">{0}</label></div><div class=\"k-edit-field\"><select id=\"required\" multiple=\"multiple\" data-placeholder=\"{1}\"...\"></select></div>";
        //    template += "<script>";
        //    template += "$(\"#required\").kendoMultiSelect({{dataTextField: \"{2}\", dataValueField: \"{3}\", dataSource: {{transport: {{read: {{dataType: \"json\", url: \"{4}\"}} }} }}, height: 300}}); ";
        //    template += "var multiSelect = $(\"#required\").data(\"kendoMultiSelect\"), setValue = function(e) {{if (e.type != \"keypress\" || kendo.keys.ENTER == e.keyCode) {{multiSelect.dataSource.filter({{}}); multiSelect.value($(\"#required\").val().split(\",\")); }} }};";
        //    template += "</script>";

        //    //template += "setValue = function(e) {{if (e.type != \"keypress\" || kendo.keys.ENTER == e.keyCode) {{required.dataSource.filter({{}}); required.value($(\"#required\").val().split(\",\")); }}  }},";

        //    //template += "$(\"#required\").kendoMultiSelect({dataTextField: \"{2}\", dataValueField: \"{3}\",dataSource: {transport: {read: {dataType: \"jsonp\", url: \"{4}\"} } }, height: 300});";
        //    //headerTemplate: '<div class="dropdown-header">' +
        //    //        '<span class="k-widget k-header">Photo</span>' +
        //    //        '<span class="k-widget k-header">Contact info</span>' +
        //    //    '</div>',
        //    //itemTemplate: '<span class="k-state-default"><img src=\"../content/web/Customers/${data.CustomerID}.jpg\" alt=\"#:data.CustomerID#\" /></span>' +
        //    //          '<span class="k-state-default"><h3>#: data.ContactName #</h3><p>#: data.CompanyName #</p></span>',
        //    //tagTemplate:  '<img class="tag-image" src=\"../content/web/Customers/${data.CustomerID}.jpg\" alt=\"${data.CustomerID}\" />' +
        //    //              '#: data.ContactName #',
        //    template = string.Format(template, label, placeholder, datatextfield, datavaluefield, url);
        //    return template;
        //}

        //[HttpPost]
        //public HttpResponseMessage GetNewElabForm(dynamic data)
        //{
        //    var response = new HttpResponseMessage();

        //    int JO_TIPJOB_ID = data.JO_TIPJOB_ID;
        //    int? JO_JOBANA_JO_EXEORD_ID = data.JO_JOBANA_JO_EXEORD_ID;
        //    int? JO_JOBANA_ID = data.JO_JOBANA_ID;
        //    bool isfirstep = data.isfirststep;

        //    // get first step for selected tipjob
        //    List<Data.JO_EXEORD_execution_order> res = (from e in _context.JO_EXEORD_execution_order.Where(x => x.JO_EXEORD_JO_TIPJOB_ID == JO_TIPJOB_ID).OrderBy(x => x.JO_EXEORD_ORDINE)
        //                                                select e).ToList();                       

        //    if (res.Count() > 0)
        //    {
        //        Data.JO_EXEORD_execution_order r;

        //        if (isfirstep)
        //        {
        //            r = res.First();
        //        }
        //        else
        //        {
        //            r = res.Where(x => x.JO_EXEORD_ID == (int)JO_JOBANA_JO_EXEORD_ID).FirstOrDefault();
        //        }
                           
        //        // build html for popup window for starting the process
        //        HtmlGenericControl container = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.div, null, null, null, "fldcontainer");
        //        string toappend = "";

        //        if (isfirstep == true)
        //        {
        //            toappend = getHtmlformultiselect("Gruppi", "Seleziona gruppi...", "Textfield", "Valuefield", @"/api/Elaborazioni/GetGroups");
        //            container.InnerHtml += toappend;
        //        }

        //        // get fields for selected step
        //        List<Data.JO_JOBFIE_job_fields> fields = (from e in _context.JO_JOBFIE_job_fields.Where(x => x.JO_JOBFIE_JO_EXEORD_ID == r.JO_EXEORD_ID)
        //                                                  select e).ToList();
                
        //        if (fields.Count() > 0)
        //        {
        //            // get templates for fields from magicframework
        //            List<MagicFramework.Data.Magic_TemplateDataRoles> templates = (from e in magic.Magic_TemplateDataRoles
        //                                                                           select e).ToList();                              
        //            foreach (var f in fields)
        //            {
        //                switch (f.JO_JOBFIE_COLUMN_TYPE)
        //                {
        //                    case "int":
        //                        toappend = getHtmlfordatarole("number", templates);
        //                        toappend = string.Format(toappend, f.JO_JOBFIE_COLUMN_NAME, f.JO_JOBFIE_COLUMN_NAME, null);
        //                        break;
        //                    case "bit":
        //                        toappend = getHtmlfordatarole("checkbox", templates);
        //                        toappend = string.Format(toappend, f.JO_JOBFIE_COLUMN_NAME, f.JO_JOBFIE_COLUMN_NAME, null);
        //                        break;
        //                    case "datetime":
        //                        toappend = getHtmlfordatarole("datetimepicker", templates);
        //                        toappend = string.Format(toappend, f.JO_JOBFIE_COLUMN_NAME, "date", f.JO_JOBFIE_COLUMN_NAME, null);
        //                        break;
        //                    case "file":
        //                        toappend = getHtmlfordatarole("applicationupload", templates);
        //                        toappend = string.Format(toappend, f.JO_JOBFIE_COLUMN_NAME, f.JO_JOBFIE_COLUMN_NAME);
        //                        break;
        //                    case "string":
        //                        toappend = getHtmlfordatarole("text", templates);
        //                        toappend = string.Format(toappend, f.JO_JOBFIE_COLUMN_NAME, "text", f.JO_JOBFIE_COLUMN_NAME, null);
        //                        break;
        //                    default:
        //                        toappend = getHtmlfordatarole("text", templates);
        //                        toappend = string.Format(toappend, f.JO_JOBFIE_COLUMN_NAME, "text", f.JO_JOBFIE_COLUMN_NAME, null);
        //                        break;
        //                }
        //                container.InnerHtml += toappend;
        //            }
        //        }
        //        HtmlGenericControl butdiv = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.div, "k-edit-field", null, null, null);
        //        HtmlButton but = new HtmlButton();
        //        but.Attributes.Add("class", "k-button");
        //        but.InnerHtml = "Save";
        //        string parameters = isfirstep ? isfirstep.ToString().ToLower() : isfirstep.ToString().ToLower() + "," + JO_JOBANA_ID.ToString();
        //        but.Attributes.Add("onclick", "but_submit(" + parameters + ");");
        //        butdiv.Controls.Add(but);
        //        container.Controls.Add(butdiv);
        //        response.StatusCode = HttpStatusCode.OK;
        //        if ((!isfirstep) && (fields.Count() == 0))
        //        {
        //            response.Content = new StringContent("");
        //        }   
        //        else
        //        {
        //            response.Content = new StringContent(HtmlControlsBuilder.HtmlControlToText(container));
        //        }
        //    }
        //    else
        //    {
        //        response.StatusCode = HttpStatusCode.InternalServerError;
        //        response.Content = new StringContent("Nessuno step definito per il processo");
        //    }

        //    return response;
        //}


        void addjobfield(List<Data.JO_JOBFIE_job_fields> fie, dynamic data, Data.JO_JOBANA_job_steps jobana) {
            foreach (var f in fie)
            {
                foreach (var d in data.inputdata)
                {
                    if (d.name == f.JO_JOBFIE_COLUMN_NAME)
                    {
                        Data.JO_JOBINP_job_input jobinp = new Data.JO_JOBINP_job_input();
                        jobinp.JO_JOBANA_job_steps = jobana;
                        jobinp.JO_JOBFIE_job_fields = f;
                        jobinp.JO_JOBINP_VALUE = d.value;
                        _context.JO_JOBINP_job_input.Add(jobinp);
                    }
                }
            }
        }

        private XmlDocument updatexmlparams(dynamic data,Data.JO_JOBANA_job_steps jobana) {
            // build the xmlinput parameters
            data.cfgDataSourceCustomParam = "";
            data.JO_JOBANA_ID = jobana.JO_JOBANA_ID;
            if (data.inputdata != null)
            {
                foreach (var d in data.inputdata)
                    data[d.name.ToString()] = d.value;
            }
                string infos = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var xml = JsonUtils.Json2Xml(infos,
                    "update",
                    "runjob",
                    jobana.JO_JOBANA_JO_EXEORD_ID.ToString(),
                    jobana.JO_JOBANA_JO_EXEORD_ID.ToString(),
                    0,
                    0,
                    -1,
                    null,
                    null,
                    null,
                    -1,
                    null,
                    null);
                jobana.JO_JOBANA_XMLPARAMS = xml.OuterXml;
          _context.SaveChanges();
         return xml;
          
        }

        [HttpPost]
        public HttpResponseMessage RunJob(dynamic data) {
            int JO_JOBANA_ID = data.JO_JOBANA_ID;
            //int JO_JOBANA_ID = data.id;
            Data.JO_JOBANA_job_steps jobana = (from e in _context.JO_JOBANA_job_steps.Where(x => x.JO_JOBANA_ID == JO_JOBANA_ID)
                                                     select e).FirstOrDefault();

            jobana.JO_JOBANA_START_DATE = DateTime.Now;
            jobana.JO_JOBANA_USER_ID = MagicFramework.Helpers.SessionHandler.IdUser;

            var fie = (from e in _context.JO_JOBFIE_job_fields.Where(x => x.JO_JOBFIE_JO_EXEORD_ID == jobana.JO_JOBANA_JO_EXEORD_ID)
                       select e).ToList();

            var inputs = (from e in _context.JO_JOBINP_job_input.Where(x => x.JO_JOBINP_JO_JOBANA_ID == JO_JOBANA_ID)
                          select e.JO_JOBINP_ID).ToList();

            if (fie.Count() > 0 && inputs.Count()==0) // se ho gia' creato gli input non li ricreo
            {
                addjobfield(fie, data, jobana);
            }

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                _context.SaveChanges();

                if (inputs.Count() == 0)
                    updatexmlparams(data, jobana);
            }
            catch (Exception ex)
            {
                MFLog.LogInFile("Elaborazioni.RunJob: " + ex.Message, MFLog.logtypes.ERROR);
                response = MagicFramework.Helpers.Utils.retInternalServerError(string.Format("Errore nel lancio dell'elaborazione: {0}", ex.Message));
                return response;
                throw;
            }
           
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        [HttpPost]
        public HttpResponseMessage RunFirstJob(dynamic data)
        {
            // selected TIPJOB_ID           
            int id = data.JO_TIPJOB_ID;

            int[] groups = new int[0];
            if (data.groups != null)
            { 
                groups = ((Newtonsoft.Json.Linq.JArray)data.groups).Select(jv => (int)jv).ToArray();    // gruppi selezionati
            }

            // get the list of associated EXEORD
            List<Data.JO_EXEORD_execution_order> r = (from e in _context.JO_EXEORD_execution_order.Where(x => x.JO_EXEORD_JO_TIPJOB_ID == id).OrderBy(x => x.JO_EXEORD_ORDINE)
                                                      select e).ToList();

            // get first step for selected tipjob
            Data.JO_EXEORD_execution_order res = r.First();
            
            DateTime now = DateTime.Now;

            Data.JO_JOBEVE_jobs jobeve = new Data.JO_JOBEVE_jobs();
            jobeve.JO_JOBEVE_START_DATE = now;
            jobeve.JO_JOBEVE_JO_TIPJOB_ID = res.JO_EXEORD_JO_TIPJOB_ID;
            jobeve.JO_JOBEVE_JO_LAST_EXEORD_ID = res.JO_EXEORD_JO_STAJOB_ID;    // si chiama così il campo ma non relaziona EXEORD, ma STAJOB ...(??)        
            jobeve.JO_JOBEVE_USER_ID = MagicFramework.Helpers.SessionHandler.IdUser;
            _context.JO_JOBEVE_jobs.Add(jobeve);

            // add list of selected gorups
            foreach (int i in groups)
            {
                Data.JO_JOBGRO_Job_groups jobgro = new Data.JO_JOBGRO_Job_groups();
                jobgro.JO_JOBEVE_jobs = jobeve;                
                jobgro.JO_JOBGRO_US_GROUPS_ID = i;
                jobgro.JO_JOBGRO_US_AREVIS_ID = MagicFramework.Helpers.SessionHandler.UserVisibilityGroup;
                _context.JO_JOBGRO_Job_groups.Add(jobgro);
            }

            //// add first EXEORD to JOBEVE
            //Data.JO_JOBANA_job_steps jobana = new Data.JO_JOBANA_job_steps();
            //jobana.JO_JOBEVE_jobs = jobeve;            
            //jobana.JO_JOBANA_START_DATE = now;
            //jobana.JO_JOBANA_JO_EXEORD_ID = res.JO_EXEORD_ID;
            ////jobana.JO_JOBANA_XMLPARAMS = MagicFramework.Helpers.JsonUtils.Json2Xml(text).OuterXml;
            //jobana.JO_JOBANA_USER_ID = MagicFramework.Helpers.SessionHandler.IdUser;
            //_context.JO_JOBANA_job_steps.Add(jobana);            

            //// add resto of EXEORD
            //r.RemoveAt(0);

            int c = 1;

            var jobana = new Data.JO_JOBANA_job_steps();

            foreach (Data.JO_EXEORD_execution_order exeord in r)
            {
                Data.JO_JOBANA_job_steps _jobana = new Data.JO_JOBANA_job_steps();
                _jobana.JO_JOBEVE_jobs = jobeve;
                _jobana.JO_JOBANA_JO_EXEORD_ID = exeord.JO_EXEORD_ID;
                _context.JO_JOBANA_job_steps.Add(_jobana);

                if (c == 1) 
                {
                    jobana = _jobana;
                }
                c++;
            }
            var fie = (from e in _context.JO_JOBFIE_job_fields.Where(x => x.JO_JOBFIE_JO_EXEORD_ID == res.JO_EXEORD_ID)
                       select e).ToList();

            addjobfield(fie, data, jobana);

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                _context.SaveChanges();
                var xml = updatexmlparams(data, jobana);
            }        

             catch (Exception ex)
            {
                MFLog.LogInFile("Elaborazioni.RunFirstJob: " + ex.Message, MFLog.logtypes.ERROR);
                response = MagicFramework.Helpers.Utils.retInternalServerError(string.Format("Errore nel lancio dell'elaborazione: {0}", ex.Message));
                return response;
                throw;
            }
          
            response.StatusCode = HttpStatusCode.OK;
            return response;

        }

        [HttpPost]
        public HttpResponseMessage SaveJobanaProare(dynamic data)
        {

            HttpResponseMessage response = new HttpResponseMessage();

            if (data.JO_ANAPRO_JO_PROARE_ID != null)
            {
                int JO_ANAPRO_JO_PROARE_ID = data.JO_ANAPRO_JO_PROARE_ID;
                int JO_ANAPRO_JO_JOBANA_ID = data.JO_JOBANA_ID;
                
                Data.JO_ANAPRO_jobana_proare anapro = new Data.JO_ANAPRO_jobana_proare();
                anapro.JO_ANAPRO_JO_JOBANA_ID = JO_ANAPRO_JO_JOBANA_ID;
                anapro.JO_ANAPRO_JO_PROARE_ID = JO_ANAPRO_JO_PROARE_ID;
                _context.JO_ANAPRO_jobana_proare.Add(anapro);

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Content = new StringContent(string.Format("Errore nel salvataggio dati id {0} ", JO_ANAPRO_JO_PROARE_ID.ToString()));
                    throw;
                }
            }

            response.StatusCode = HttpStatusCode.OK;
            return response;

        }

        [HttpPost]
        public HttpResponseMessage UpdateJOBREC(int id,dynamic data)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response = Utils.retOkJSONMessage("OK!");
            if (data.JO_JOBREC_ID != null)
            { 
                //int id = data.JO_JOBREC_ID;
                DateTime dt = DateTime.Now;
                Data.JO_JOBREC_job_record jobrec = (from e in _context.JO_JOBREC_job_record.Where(x => x.JO_JOBREC_ID == id)
                                                    select e).FirstOrDefault();

                jobrec.JO_JOBREC_SELECTED = data.JO_JOBREC_SELECTED;
                jobrec.JO_JOBRED_USERID = MagicFramework.Helpers.SessionHandler.IdUser;
                jobrec.JO_JOBREC_DATE = dt;
                
                try
                {
                    _context.SaveChanges();                
                }
                catch (Exception)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Content = new StringContent(string.Format("Errore nel salvataggio dati id {0} ", id.ToString()));        
                    throw;
                }
            }

            return response;    
          
       
        }
    }
}