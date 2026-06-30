#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornGeneratedAssetImportSanitizer
    {
        private const string AutoImportedRoot = "Assets/Elementborn/Art/Models/MeshyImported/AutoImported";
        private const string QuarantineRoot = "QuarantinedGeneratedAssetFiles";

        [MenuItem("Elementborn/Assets/Sanitize Generated Asset Imports")]
        public static void SanitizeGeneratedAssetImports()
        {
            string projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
            string absoluteRoot = Path.Combine(projectRoot, AutoImportedRoot);
            string quarantineRoot = Path.Combine(projectRoot, QuarantineRoot);

            if (!Directory.Exists(absoluteRoot))
            {
                Debug.LogWarning("No generated auto-import folder found: " + AutoImportedRoot);
                return;
            }

            Directory.CreateDirectory(quarantineRoot);

            int checkedFiles = 0;
            int quarantined = 0;

            string[] files = Directory.GetFiles(absoluteRoot, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string extension = Path.GetExtension(file).ToLowerInvariant();

                bool shouldCheck =
                    extension == ".png" ||
                    extension == ".jpg" ||
                    extension == ".jpeg";

                if (!shouldCheck)
                {
                    continue;
                }

                checkedFiles++;

                if (!LooksLikeReadableImage(file, extension))
                {
                    QuarantineFile(projectRoot, file, quarantineRoot);
                    quarantined++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Generated asset import sanitize complete. Checked image files=" + checkedFiles + " Quarantined=" + quarantined);
        }

        private static bool LooksLikeReadableImage(string path, string extension)
        {
            try
            {
                FileInfo info = new FileInfo(path);
                if (!info.Exists || info.Length < 16)
                {
                    return false;
                }

                byte[] header = new byte[12];
                using (FileStream stream = File.OpenRead(path))
                {
                    int read = stream.Read(header, 0, header.Length);
                    if (read < 4)
                    {
                        return false;
                    }
                }

                if (extension == ".png")
                {
                    return
                        header[0] == 0x89 &&
                        header[1] == 0x50 &&
                        header[2] == 0x4E &&
                        header[3] == 0x47 &&
                        header[4] == 0x0D &&
                        header[5] == 0x0A &&
                        header[6] == 0x1A &&
                        header[7] == 0x0A;
                }

                if (extension == ".jpg" || extension == ".jpeg")
                {
                    return header[0] == 0xFF && header[1] == 0xD8;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void QuarantineFile(string projectRoot, string file, string quarantineRoot)
        {
            string relative = file.Replace("\\", "/").Replace(projectRoot.Replace("\\", "/"), "");
            string safeRelative = relative.Replace(":", "").Replace("/", "__").Replace("\\", "__");
            string destination = Path.Combine(quarantineRoot, safeRelative);

            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            File.Move(file, destination);

            string meta = file + ".meta";
            if (File.Exists(meta))
            {
                string metaDestination = destination + ".meta";
                if (File.Exists(metaDestination))
                {
                    File.Delete(metaDestination);
                }

                File.Move(meta, metaDestination);
            }

            Debug.LogWarning("Quarantined unreadable generated asset image: " + relative + " -> " + destination);
        }
    }
}
#endif
