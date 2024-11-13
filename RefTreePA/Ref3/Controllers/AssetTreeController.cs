using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Data.SqlClient;
using MagicFramework.Helpers;
using System.Data;

namespace Ref3.Controllers
{
    public class AssetTreeController : ApiController
    {
        private Data.RefTreeEntities refdata = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();

        // POST api/<controller>
        [HttpPost]
        public string Post(dynamic data)
        {
            
            int asset_id = data.asset_id;

         //   int root = (int)refdata.AS_GET_ASSET_MAIN_TB(asset_id).FirstOrDefault().AS_ASSET_ID;

            var assets = refdata.GET_ASSET_TREE(asset_id).ToList();
                         
             string JSON = String.Empty;

             var currentasset = assets.Where(x => x.ASSET_ID == asset_id).FirstOrDefault();
             if (currentasset != null)
             {
                    JSON += buildTree(asset_id, assets, currentasset, asset_id, Request);
             }
            return JSON;

        }

        [HttpPost]
        public IList<int> PostRestrictions(dynamic data)
        {

            int asset_id = data.assetid;
            var tipsas = refdata.GetAllowedTipSasForAsset(asset_id).Select(l=> l.Value).ToList();

            return tipsas;
        }

        [HttpPost]
        public IList<string> PostHiddenDetails(dynamic data)
        {

            int tipsas_id = data.tipsas_id;
            var tipsas = refdata.GET_TIPSAS_DETAILTOHIDE(tipsas_id).ToList(); 
            return tipsas;
        }

        public string buildTree(int  selectedassetid,IEnumerable<Data.GET_ASSET_TREE_Result> assets,Data.GET_ASSET_TREE_Result currentasset, int startingpointassetid, HttpRequestMessage Request)
        {
            Uri MyUrl = null;
            if (Request != null)
                MyUrl = Request.RequestUri;

            var children = assets.Where(x => x.ASSET_ID_PADRE == startingpointassetid).OrderBy(y=> y.ASSET_ID);
            int childrencounter = children.Count();

            string selected = "false";
            if (selectedassetid == startingpointassetid)
                selected = "true";

            string toadd = String.Empty;
            toadd += "{\"LayerID\":\"" + currentasset.MagicBOLayerID + "\",\"GridName\":\"" + currentasset.MagicGridName + "\",\"EditableTemplateID\":\"" + currentasset.MagicTemplateID + "\",\"assettipassid\":\"" + currentasset.AS_ASSET_TIPASS_ID + "\",\"assettipsasid\":\"" + currentasset.AS_ASSET_TIPSAS_ID + "\",\"assetdescr\":\"" + currentasset.ASSET_DESCRIZIONE + "\",\"type\":\"" + currentasset.AS_TIPSAS_DESCRIZIONE + "\",\"assetgroup\":\"" + currentasset.AS_TIPSAS_DESCRIZIONE + "\",\"assetcode\":\"" + currentasset.ASSET_CODE + "\",\"assetid\":\"" + currentasset.ASSET_ID + "\",\"parentassetid\":\"" + currentasset.ASSET_ID_PADRE + "\",\"assetlocationid\":\"" + currentasset.AS_ASSET_LOCATION_ID + "\",\"selected\":"+selected+",\"expanded\":true,\"imageUrl\":\"http://" + MyUrl.Authority + "/Views/Images/" + currentasset.AS_TIPSAS_ICONA + "\",\"items\":[";

            int i = 0;
            if (childrencounter > 0)
            {
                foreach (var assetchild in children)
                {
                    toadd += buildTree(selectedassetid,assets, assetchild,(int)assetchild.ASSET_ID, Request);

                    if (i < childrencounter)
                    {
                        toadd += ",";//virgola tra i children
                    }
                    i++;
                }
                toadd = toadd.Substring(0, toadd.Length - 1) + "]}";
            }
            else toadd += "]}";
            return toadd;
        }
        
    }
}