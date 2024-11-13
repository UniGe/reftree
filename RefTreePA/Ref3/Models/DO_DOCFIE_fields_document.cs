using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework.Helpers;
using System.IO;


namespace Ref3.Data
{
    public partial class DO_DOCFIE_fields_document
    {
        private const string layerRoot = "DO_TIP_";
        private const string layerClaRoot = "DO_CLA_";
        private const string layerType = "XMLDATA";
        //public bool filldata(Data.DO_V_DOCUME inputdata, ref Data.DO_DOCUME_documents entityupdate)
        private string getLayerCode(string cod, string id,bool getclass)
        {
            if (!getclass)
                return layerRoot + "_" + cod + "_" + id;
            else
                return layerClaRoot + "_" + cod + "_" + id;
        }
        private int? getClaDocLayer(int tipdoc,   Data.RefTreeEntities _context,MagicFramework.Data.MagicDBDataContext _magiccontext)
        {
            var clado = (from e in _context.DO_TIPDOC_document_type where e.DO_TIPDOC_ID == tipdoc select e.DO_CLADOC_document_class).First();
            string cod = this.getLayerCode(clado.DO_CLADOC_CODE,clado.DO_CLADOC_ID.ToString(),true);
            int? layerid = (from e in _magiccontext.Magic_ApplicationLayers where e.LayerCode == cod select e.LayerID).FirstOrDefault();
            return layerid;
        }

        public bool filldata(Data.DO_DOCFIE_fields_document inputdata)
        {
            try
            {
                // TODO: Commenti 20.06.2015 Daniele Bisol dopo cambio db DOCFIE Marco/Dario
                // (chiedere) e in caso aggiornare il tab nelle configurazioni dei documenti
                this.DO_DOCFIE_CF_FIELDS_DATA_TYPE_ID = inputdata.DO_DOCFIE_CF_FIELDS_DATA_TYPE_ID;
                //this.DO_DOCFIE_DESCRIPTION_JOIN = inputdata.DO_DOCFIE_DESCRIPTION_JOIN;
                this.DO_DOCFIE_DO_TIPDOC_ID = inputdata.DO_DOCFIE_DO_TIPDOC_ID;
                this.DO_DOCFIE_FIELD_DB = inputdata.DO_DOCFIE_FIELD_DB;
                //this.DO_DOCFIE_FIELD_JOIN = inputdata.DO_DOCFIE_FIELD_JOIN;
                this.DO_DOCFIE_ID = inputdata.DO_DOCFIE_ID;
                this.DO_DOCFIE_MagicColumnID = inputdata.DO_DOCFIE_MagicColumnID;
                //this.DO_DOCFIE_MANDATORY = inputdata.DO_DOCFIE_MANDATORY;
                //this.DO_DOCFIE_TABLE_JOIN = inputdata.DO_DOCFIE_TABLE_JOIN;
                this.DO_DOCFIE_DO_CLADOC_ID = inputdata.DO_DOCFIE_DO_CLADOC_ID;
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }

        }

        public string initializeLayer()
        {
            int? cladocid = this.DO_DOCFIE_DO_CLADOC_ID;
            int? tipdocid = this.DO_DOCFIE_DO_TIPDOC_ID;
            //se non e' un layer relativo al TIPDOC sara' relativo al CLADOC
            
            //TODO questo e' da cambiare se il MagicDB non e' SQL !!!
            MagicFramework.Data.MagicDBDataContext _magiccontext = new MagicFramework.Data.MagicDBDataContext(DBConnectionManager.GetMagicConnection()); 
            Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();

            int tipdocumento = tipdocid ?? 0;
            Data.DO_TIPDOC_document_type tip = (from e in _context.DO_TIPDOC_document_type where e.DO_TIPDOC_ID == tipdocumento select e).FirstOrDefault();
            if (tip!=null)
                cladocid = tip.DO_TIPDOC_DO_CLADOC_ID;
            
            Data.DO_CLADOC_document_class cla = (from e in _context.DO_CLADOC_document_class where e.DO_CLADOC_ID == cladocid select e).First();
       
            //creo il layer del cladoc (anche se non ci sono campi)
            string layercode = getLayerCode(cla.DO_CLADOC_CODE, cla.DO_CLADOC_ID.ToString(), true);
            var layer = (from e in _magiccontext.Magic_ApplicationLayers where e.LayerCode == layercode select e).FirstOrDefault();
            int? parentlayerID = null;
            MagicFramework.Data.Magic_AppLayersTypes laytyp = (from e in _magiccontext.Magic_AppLayersTypes where e.Code == layerType select e).Single();
            if (layer == null) // il layer non esiste, lo creo 
            {
                //aggiungo il layer 
                 MagicFramework.Data.Magic_ApplicationLayers lay = new MagicFramework.Data.Magic_ApplicationLayers();
                lay.LayerCode = layercode;
                lay.LayerDescription = "Layer of " + layercode;
                lay.LayerType_ID = laytyp.ID;
                lay.ParentLayer_ID = parentlayerID;
                _magiccontext.Magic_ApplicationLayers.InsertOnSubmit(lay);
                _magiccontext.SubmitChanges();
               //aggiorno il layerID sulla cladoc
                cla.MagicBOLayer_ID = lay.LayerID;
                _context.SaveChanges();
                layer = lay;      
            }
            string cladolayercode = layer.LayerCode;
            string tipdoclayercode = null;
            //creo il layer della tipologia (solo se e' stata indicata per un qualche campo)
            if (tip != null)
            {
                tipdoclayercode = getLayerCode(tip.DO_TIPDOC_CODE, tip.DO_TIPDOC_ID.ToString(), false);
                var layertip = (from e in _magiccontext.Magic_ApplicationLayers where e.LayerCode == tipdoclayercode select e).FirstOrDefault();
                MagicFramework.Data.Magic_ApplicationLayers lay = new MagicFramework.Data.Magic_ApplicationLayers();
                if (layertip == null)
                {
                    lay.LayerCode = tipdoclayercode;
                    lay.LayerDescription = "Layer of " + tipdoclayercode;
                    lay.LayerType_ID = laytyp.ID;
                    lay.ParentLayer_ID = layer.LayerID; //cladoc layer 
                    _magiccontext.Magic_ApplicationLayers.InsertOnSubmit(lay);
                    _magiccontext.SubmitChanges();
                    //Aggiorno il LayerID sulla tipDOC  
                    tip.MagicBOLayer_ID = lay.LayerID;
             
                    _context.SaveChanges();
                }
            }

            if (tip != null)
                return tipdoclayercode;
            else
                return cladolayercode;
        }
    }
}


