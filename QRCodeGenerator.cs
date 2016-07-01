        public void QrCodeGenerator()
        {
            UIDocument uidoc = this.ActiveUIDocument;
            Document doc = uidoc.Document;
            
            List<ElementId> ids = new List<ElementId>();
            Selection sel = uidoc.Selection;

            ICollection<ElementId> selIds = sel.GetElementIds();

            if (0 < selIds.Count)
            {
                foreach (ElementId id in selIds)
                {
                    ids.Add(id);
                }
            }

            if (0 == selIds.Count)
            {
                IList<Reference> refs = null;

                try
                {
                    refs = sel.PickObjects(ObjectType.Element,
                    "Please Element(s) to Generate QrCode.");
                }
                catch (Autodesk.Revit.Exceptions
                .OperationCanceledException)
                {
                    //  return Result.Cancelled;
                }
                ids = new List<ElementId>(
                refs.Select<Reference, ElementId>(
                r => r.ElementId));
            }

            foreach (ElementId id in ids)
            {
                Element el = doc.GetElement(id);
                FamilyInstance elsym = el as FamilyInstance;
                FamilySymbol famsym = elsym.Symbol as FamilySymbol;
                string data = Uri.EscapeUriString(famsym.get_Parameter(BuiltInParameter.ALL_MODEL_URL).AsString());
                string folderpath = @"C:\Your Folder Path\";
                string Filename = el.Name;

                GenerateQRCode(data, folderpath, Filename);
            }
        }

        private static void GenerateQRCode(string Data, string folderpath, string fileName)
        {
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("https://api.qrserver.com/v1/create-qr-code/?size=900x900&data=" + Data);
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                if ((response.StatusCode == System.Net.HttpStatusCode.OK ||
                    response.StatusCode == System.Net.HttpStatusCode.Moved ||
                    response.StatusCode == System.Net.HttpStatusCode.Redirect) &&
                    response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {
                    using (System.IO.Stream inputStream = response.GetResponseStream())
                    using (System.IO.Stream outputStream = System.IO.File.OpenWrite(folderpath + fileName + ".png"))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        do
                        {
                            bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                            outputStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead != 0);
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }