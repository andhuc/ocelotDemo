using System.IO;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Contract.Services.Controllers;
using iText.Kernel.Geom;

namespace Contract.Service.Models.Implements
{
    public class SignService : ISignService
    {
        public readonly string KEYSTORE = "Cert/key.p12";
        public readonly char[] PASSWORD = "badssl.com".ToCharArray();

        public void SignPdf(string destPath, List<Signature> signatures, Contract contract)
        {
            Pkcs12Store pk12 = new Pkcs12StoreBuilder().Build();
            pk12.Load(new FileStream(KEYSTORE, FileMode.Open, FileAccess.Read), PASSWORD);
            string alias = null;
            foreach (var a in pk12.Aliases)
            {
                alias = ((string)a);
                if (pk12.IsKeyEntry(alias))
                    break;
            }

            ICipherParameters pk = pk12.GetKey(alias).Key;
            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
            X509Certificate[] chain = new X509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
            {
                chain[k] = ce[k].Certificate;
            }

            File.Copy(contract.Path, destPath + $"{contract.Id}_0.pdf", true);
            int version = 0;

            foreach (var signature in signatures)
            {
                string srcPath = destPath + $"{contract.Id}_{version}.pdf";
                string signedPath = destPath + $"{contract.Id}_{++version}.pdf";

                if (signature == signatures.Last())
                    signedPath = destPath + $"{contract.Id}.pdf";

                Sign(srcPath, signedPath, chain, pk,
                    DigestAlgorithms.SHA256,
                    PdfSigner.CryptoStandard.CMS, signature);
            }
        }

        private void Sign(String src, String dest, X509Certificate[] chain,
            ICipherParameters pk, String digestAlgorithm, PdfSigner.CryptoStandard subfilter,
            Signature signature)
        {
            PdfReader reader = new PdfReader(src);

            reader.SetUnethicalReading(true);

            // Pass the temporary file's path to the PdfSigner constructor
            using (FileStream fs = new FileStream(dest, FileMode.Create))
            {
                PdfSigner signer = new PdfSigner(reader, fs, false);

                // Create the signature appearance
                Rectangle rect = new Rectangle(signature.X, signature.Y, signature.Width, signature.Height);
                PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
                appearance
                    .SetReason(signature.Reason)
                    .SetLocation("HN")
                    .SetReuseAppearance(false)
                    .SetPageRect(rect)
                    .SetPageNumber(signature.Page);

                appearance.SetLayer2Text(signature.Name);

                IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);

                // Sign the document using the detached mode, CMS, or CAdES equivalent.
                signer.SignDetached(pks, chain, null, null, null, 0, subfilter);
            }
        }


    }
}
