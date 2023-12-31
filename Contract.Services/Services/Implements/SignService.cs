﻿using System.IO;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using iText.Kernel.Geom;
using iText.IO.Font;
using iText.Kernel.Font;

namespace Contract.Service.Models.Implements
{
    public class SignService : ISignService
    {
        public readonly string KEYSTORE = "Cert/key.p12";
        public readonly char[] PASSWORD = "badssl.com".ToCharArray();

        public void SignMany(string destPath, List<Signature> signatures, Contract contract)
        {
            // Input and output file paths
            string inputPath = destPath + $"{contract.Id}_0.pdf";
            string outputPath = destPath + $"{contract.Id}.pdf";

            // Copy the original contract to a temporary file
            File.Copy(contract.Path, inputPath, true);

            foreach (Signature signature in signatures)
            {
                // Sign the temporary file with the signature
                Sign(inputPath, outputPath, signature);

                // Copy the signed file back to the temporary file for multiple signatures
                File.Copy(outputPath, inputPath, true);
            }
        }

        private void Sign(string inputPath, string outputPath, Signature signature)
        {
            // Load the keystore
            Pkcs12Store pk12 = new Pkcs12StoreBuilder().Build();
            pk12.Load(new FileStream(KEYSTORE, FileMode.Open, FileAccess.Read), PASSWORD);
            string alias = null;

            // Find the alias for the key entry in the keystore
            foreach (var a in pk12.Aliases)
            {
                alias = ((string)a);
                if (pk12.IsKeyEntry(alias))
                    break;
            }

            // Get the private key and certificate chain from the keystore
            ICipherParameters pk = pk12.GetKey(alias).Key;
            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
            X509Certificate[] chain = new X509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
            {
                chain[k] = ce[k].Certificate;
            }

            // Sign the PDF with the provided signature information
            SignAppearance(inputPath, outputPath, chain, pk,
                    DigestAlgorithms.SHA256,
                    PdfSigner.CryptoStandard.CMS, signature);
        }

        private void SignAppearance(String src, String dest, X509Certificate[] chain,
            ICipherParameters pk, String digestAlgorithm, PdfSigner.CryptoStandard subfilter,
            Signature signature)
        {
            // Read the original PDF
            PdfReader reader = new PdfReader(src);
            reader.SetUnethicalReading(true);

            // Pass the temporary file's path to the PdfSigner constructor
            using (FileStream fs = new FileStream(dest, FileMode.Create))
            {
                // Create a PdfSigner instance
                PdfSigner signer = new PdfSigner(reader, fs, true);

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

                // Set the font for the appearance (choose a font that supports UTF-8 characters)
                PdfFont font = PdfFontFactory.CreateFont("Cert/font.ttf", PdfEncodings.IDENTITY_H, true);
                appearance.SetLayer2Font(font);

                // Create a private key signature
                IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);

                // Sign the document using the detached mode, CMS, or CAdES equivalent.
                signer.SignDetached(pks, chain, null, null, null, 0, subfilter);
            }
        }


    }
}
