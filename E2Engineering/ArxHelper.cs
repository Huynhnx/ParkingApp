using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ParkingApp
{
    public enum BlockRefType
    {
        None = 0,
        Xref,
        Block,
    };
    class ArxHelper
    {
        public static double _PI = 3.14159265358979323846;
        public static double _2PI = 6.28318530717958647693;
        public static double _HALFPI = 1.57079632679489661923;
        public static Tolerance Tol = new Tolerance(0.0872222222222222222222, 0.0872222222222222222222);
        public static ObjectId AppendEntity(Entity ent)
        {
            ObjectId objId = ObjectId.Null;
            if (ent == null)
            {
                return objId;
            }
            Document doc = Application.DocumentManager.MdiActiveDocument;
            try
            {
                using (doc.LockDocument())
                {
                    Database db = doc.Database;
                    // start new transaction
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        // open model space block table record
                        BlockTableRecord spaceBlkTblRec = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                        // append entity to model space block table record
                        objId = spaceBlkTblRec.AppendEntity(ent);
                        trans.AddNewlyCreatedDBObject(ent, true);
                        // finish
                        trans.Commit();
                    }
                }

            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage(ex.ToString());
            }
            return objId;
        }
        public static void DeleteEntity(ObjectId id)
        {
            if (ObjectId.Null == id || id.IsErased)
                return;
            using (Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Database db = Application.DocumentManager.MdiActiveDocument.Database;
                // start new transaction
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    using (DBObject obj = trans.GetObject(id, OpenMode.ForWrite))
                    {
                        if (null != obj && !obj.IsErased)
                        {
                            obj.Erase();
                        }
                    }
                    // finish
                    trans.Commit();
                }
            }
        }
        public static void AddRegAppTableRecord(string regAppName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            using (Transaction tr = doc.TransactionManager.StartOpenCloseTransaction())
            {

                RegAppTable rat = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForWrite, false);
                if (!rat.Has(regAppName))
                {
                    //rat.UpgradeOpen();

                    RegAppTableRecord ratr = new RegAppTableRecord();
                    ratr.Name = regAppName;
                    rat.Add(ratr);
                    tr.AddNewlyCreatedDBObject(ratr, true);
                }
                tr.Commit();
            }
        }
        static public ObjectIdCollection ReadXdata(Entity ent)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ResultBuffer rb = ent.GetXDataForApplication("ListBlock");
            ObjectIdCollection listId = new ObjectIdCollection();
            if (rb == null)
            {
                return null;
            }
            else
            {
                foreach (TypedValue tv in rb)
                {
                    if (tv.TypeCode == (int)DxfCode.SoftPointerId)
                    {
                        ObjectId id = (ObjectId)tv.Value;
                        if (id.IsValid && id.IsErased == false)
                        {
                            listId.Add(id);
                        }
                    }
                }
                rb.Dispose();
            }
            return listId;
        }
        public static bool WriteXdata(DBObject ent, ResultBuffer newData)
        {
            bool ret = false;
            if (ent != null)
            {
                // Name      
                ent.XData = newData;
                ret = true;
            }
            return ret;
        }
        public static void AttachDataToEntity(ObjectId entId, ResultBuffer rb, string AppName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            // now start a transaction 
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // Declare an Entity variable named ent.  
                    DBObject ent = trans.GetObject(entId, OpenMode.ForRead) as DBObject;
                    // test the IsNull property of the ExtensionDictionary of the ent. 
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        // Upgrade the open of the entity. 
                        ent.UpgradeOpen();

                        // Create the ExtensionDictionary
                        ent.CreateExtensionDictionary();
                    }
                    // variable as DBDictionary. 
                    DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite);
                    if (extensionDict != null)
                    {
                        //  Create a new XRecord. 
                        Xrecord myXrecord = new Xrecord();

                        // Add the ResultBuffer to the Xrecord 
                        myXrecord.Data = rb;
                        // Create the entry in the ExtensionDictionary. 
                        extensionDict.SetAt(AppName, myXrecord);
                        // Tell the transaction about the newly created Xrecord 
                        trans.AddNewlyCreatedDBObject(myXrecord, true);
                    }
                    // all ok, commit it 
                    trans.Commit();
                    trans.Dispose();
                }
                catch (System.Exception ex)
                {
                    trans.Abort();
                }
            }
        }
        public static bool ReadEntityData(ObjectId entId, ref ResultBuffer rb, string AppName)
        {
            bool bRet = false;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // Declare an Entity variable named ent. 
                    if (entId.IsValid && entId.IsErased == false)
                    {
                        DBObject ent = trans.GetObject(entId, OpenMode.ForRead) as DBObject;
                        if (ent != null && ent.ExtensionDictionary.IsValid && !ent.ExtensionDictionary.IsErased)
                        {
                            // Declare an Entity variable named ent.                  
                            // variable as DBDictionary.                        
                            DBDictionary extensionDict = trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                            if (extensionDict != null && extensionDict.Contains(AppName))
                            {
                                // Check to see if the entry we are going to add is already there. 
                                ObjectId entryId = extensionDict.GetAt(AppName);
                                // Extract the Xrecord. Declare an Xrecord variable. 
                                Xrecord myXrecord = default(Xrecord);
                                // Instantiate the Xrecord variable 
                                myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);
                                if (myXrecord != null && myXrecord.Data != null)
                                {
                                    rb = myXrecord.Data;

                                }
                            }
                        }
                    }
                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    trans.Abort();
                }
            }
            return bRet;
        }
        public static void CreateLayer(string Name)

        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                // Get the layer table from the drawing

                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (lt.Has(Name))
                {
                    return;
                }

                LayerTableRecord ltr = new LayerTableRecord();
                ltr.Name = Name;

                ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 151);
                lt.UpgradeOpen();
                ObjectId ltId = lt.Add(ltr);

                tr.AddNewlyCreatedDBObject(ltr, true);



                // Set the layer to be current for this drawing

                db.Clayer = ltId;

                // Commit the transaction

                tr.Commit();
            }

        }
        public static BlockRefType GetBlockReferenceType(ObjectId objId)
        {
            bool bIsBlock = false;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity acadEnt = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                if (acadEnt != null)
                {
                    if (acadEnt is BlockReference)
                    {
                        BlockReference pBlockRef = acadEnt as BlockReference;
                        if (pBlockRef != null)
                        {
                            // GET THE BLOCK TABLE RECORD ID OF THE BLOCK REFERENCE
                            ObjectId btrcd_id_inside = pBlockRef.BlockTableRecord;
                            if (btrcd_id_inside.IsValid == true && btrcd_id_inside.IsErased == false)
                            {
                                BlockTableRecord btrcd_inside = tr.GetObject(btrcd_id_inside, OpenMode.ForRead) as BlockTableRecord;
                                if (btrcd_inside != null)
                                {
                                    // CHECK IF THIS BLOCK TABLE IS FROM EXTERNAL REF
                                    if (btrcd_inside.IsFromExternalReference)
                                    {
                                        return BlockRefType.Xref;
                                    }
                                    else
                                    {
                                        return BlockRefType.Block;
                                    }
                                }
                            }
                        }
                    }
                }
                tr.Commit();
            }
            return BlockRefType.None;
        }
        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        public static void WriteRegistryIntAttribute(string regKey, string key, int val)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return;
                Microsoft.Win32.RegistryKey CurrentUser = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Registry64);
                Microsoft.Win32.RegistryKey structureFolder = CurrentUser.OpenSubKey(regKey, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);

                if (structureFolder == null)
                {
                    structureFolder = CurrentUser.CreateSubKey(regKey);
                }
                if (structureFolder != null)
                {
                    structureFolder.SetValue(key, val);
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int ReadRegistryIntAttribute(string regKey, string key)
        {
            try
            {
                Microsoft.Win32.RegistryKey CurrentUser = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Registry64);
                Microsoft.Win32.RegistryKey structureFolder = CurrentUser.OpenSubKey(regKey, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (structureFolder != null)
                {
                    var Regkey = structureFolder.GetValue(key);
                    if (Regkey != null)
                    {
                        return Convert.ToInt32(Regkey.ToString());
                    }
                }
            }
            catch { }
            return 0; // deafault AutoCAD drak
        }
        public static void ReadEntiesFromBlockRef(ObjectId objId, ObjectIdCollection objects_ids)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction tr = doc.TransactionManager.StartOpenCloseTransaction())
            {
                try
                {
                    BlockReference blrf = tr.GetObject(objId, OpenMode.ForRead) as BlockReference;
                    if (blrf != null && objects_ids != null)
                    {
                        BlockTableRecord blrk = tr.GetObject(blrf.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                        //Database xrefDB = blrk.GetXrefDatabase(true);
                        foreach (ObjectId xrefId in blrk)
                        {
                            if (xrefId.ObjectClass == RXObject.GetClass(typeof(BlockReference)))
                            {
                                ReadEntiesFromBlockRef(xrefId, objects_ids);
                            }
                            else
                            {
                                objects_ids.Add(xrefId);
                            }
                        }
                    }
                }
                catch (System.Exception)
                {
                }
            }
        }
        public static void GetAllEntities(ObjectIdCollection objects_ids)
        {
            ObjectIdCollection ids = new ObjectIdCollection();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            Transaction tr =
              db.TransactionManager.StartOpenCloseTransaction();
            using (tr)
            {
                BlockTable bt =
                  (BlockTable)tr.GetObject(
                    db.BlockTableId,
                    OpenMode.ForRead
                  );
                BlockTableRecord btr =
                  (BlockTableRecord)tr.GetObject(
                    db.CurrentSpaceId,
                    OpenMode.ForRead
                  );
                foreach (ObjectId objId in btr)
                {
                    //if (objId.ObjectClass == RXObject.GetClass(typeof(BlockReference)))
                    //{
                    //    ReadEntiesFromBlockRef(objId, objects_ids);
                    //}
                    //else
                    //{
                        objects_ids.Add(objId);
                    //}
                }
            }
        }

    }
}
