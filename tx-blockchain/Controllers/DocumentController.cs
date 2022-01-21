using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using tx_blockchain.Helpers;
using tx_blockchain.Models;

namespace tx_blockchain.Controllers
{
    public class DocumentController : Controller
    {
        const string sBlockchainPath = "App_Data/Blockchains/documents.bc";

        // this method stores the document hash in the blockchain
        [HttpPost]
        [Route("Document/StoreDocument")]
        public StoredDocument StoreDocument(string document, string uniqueId, string signerName)
        {
            byte[] bPDF;

            // create temporary ServerTextControl
            using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl())
            {
                tx.Create();

                // load the document
                tx.Load(Convert.FromBase64String(document),
                    TXTextControl.BinaryStreamType.InternalUnicodeFormat);

                TXTextControl.SaveSettings saveSettings = new TXTextControl.SaveSettings()
                {
                    CreatorApplication = "TX Text Control Sample Application",
                };

                // save the document as PDF
                tx.Save(out bPDF, TXTextControl.BinaryStreamType.AdobePDF, saveSettings);
            }

            // calculate the MD5 checksum of the binary data
            // and store in SignedDocument object
            SignedDocument signedDocument = new SignedDocument()
            {
                Hash = Checksum.CalculateMD5(bPDF),
                DocumentId = uniqueId,
                Signer = signerName
            };

            // define a Blockchain object
            Blockchain bcDocument = new Blockchain(sBlockchainPath);

            // add a new block to the blockchain and store the SignedDocument object
            if (bcDocument.AddBlock(new Block(
                DateTime.Now, 
                null, 
                JsonConvert.SerializeObject(signedDocument))) == true) {

                bcDocument.Save();

                // create and return a view model with the PDF and the unique document ID
                StoredDocument storedDocument = new StoredDocument()
                {
                    PDF = Convert.ToBase64String(bPDF),
                    BlockHash = bcDocument.GetCurrentBlock().BlockHash
                };

                return storedDocument;
            }

            return null;
        }

        // this method validates a document with the last block on the blockchain
        [HttpPost]
        [Route("Document/ValidateDocument")]
        public bool ValidateDocument(string document, string blockHash)
        {
            if (document == null || blockHash == null)
                return false;

            // calculate the MD5 of the uploaded document
            string sChecksum = Checksum.CalculateMD5(Convert.FromBase64String(document));

            // load the associated blockchain
            Blockchain bcDocument = new Blockchain(sBlockchainPath);

            Block blockDocument = bcDocument.GetBlock(blockHash);

            if (blockDocument == null)
                return false;

            if (bcDocument.IsValid(blockDocument.BlockHash))
            {

                // get the SignedDocument object from the block
                SignedDocument signedDocument =
                    JsonConvert.DeserializeObject<SignedDocument>(blockDocument.Data);

                // compare the checksum in the stored block
                // with the checksum of the uploaded document
                return (signedDocument.Hash == sChecksum ? true : false);
            }
            else return false;
        }
    }
}
