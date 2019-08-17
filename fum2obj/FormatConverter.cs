using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fum2obj
{
    class FormatConverter
    {
        private class GeometryFile
        {
            public int signature;

            public List<string> textures;

            public List<Tuple<float, float, float>> vertices;
            public List<Tuple<float, float, float>> normals;
            public List<Tuple<float, float>> mapping;

            public List<MaterialEntry> materials;

            public class MaterialEntry
            {
                public string name;

                public int unk1, unk2, unk3, unk4;

                public Tuple<float, float, float> ambient, diffuse, specular;
                public float glossiness, illuminaton, opacity;

                public List<Tuple<int, int, int>> triangles;
            }
        }

        public void ExtractGeometry(string inputFile, string outputFile)
        {
            var fumFile = new GeometryFile();

            using (BinaryReader reader = new BinaryReader(new BufferedStream(File.Open(inputFile, FileMode.Open))))
            {
                fumFile.signature = reader.ReadInt32();

                var texturesCount = reader.ReadInt32();

                fumFile.textures = new List<string>(texturesCount);

                for (int i = 0; i < texturesCount; i++)
                {
                    fumFile.textures.Add(Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32())));
                }

                var vertexCount = reader.ReadInt32();

                fumFile.vertices = new List<Tuple<float, float, float>>(vertexCount);
                fumFile.normals = new List<Tuple<float, float, float>>(vertexCount);
                fumFile.mapping = new List<Tuple<float, float>>(vertexCount);

                for (int i = 0; i < vertexCount; i++)
                {
                    fumFile.vertices.Add(new Tuple<float, float, float>(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                }

                for (int i = 0; i < vertexCount; i++)
                {
                    fumFile.normals.Add(new Tuple<float, float, float>(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                }

                for (int i = 0; i < vertexCount; i++)
                {
                    fumFile.mapping.Add(new Tuple<float, float>(reader.ReadSingle(), reader.ReadSingle()));
                }

                var materialsCount = reader.ReadInt32();

                fumFile.materials = new List<GeometryFile.MaterialEntry>(materialsCount);

                for (int i = 0; i < materialsCount; i++)
                {
                    var material = new GeometryFile.MaterialEntry();

                    material.name = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));

                    material.unk1 = reader.ReadInt32();
                    material.unk2 = reader.ReadInt32();
                    material.unk3 = reader.ReadInt32();
                    material.unk4 = reader.ReadInt32();

                    material.ambient = new Tuple<float, float, float>(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    material.diffuse = new Tuple<float, float, float>(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    material.specular = new Tuple<float, float, float>(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    material.glossiness = reader.ReadSingle();
                    material.illuminaton = reader.ReadSingle();
                    material.opacity = reader.ReadSingle();

                    var indicesCount = reader.ReadInt32();
                    var trianglesCount = indicesCount / 3;

                    material.triangles = new List<Tuple<int, int, int>>();

                    for (int j = 0; j < trianglesCount; j++)
                    {
                        material.triangles.Add(new Tuple<int, int, int>(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()));
                    }

                    fumFile.materials.Add(material);
                }
            }

            using (StreamWriter writer = new StreamWriter(new BufferedStream(File.Open(outputFile, FileMode.Create))))
            {
                writer.WriteLine("# fum2obj converter by Michael Shinoda (2019)");
                writer.WriteLine();
                
                foreach (var vertex in fumFile.vertices)
                {
                    writer.WriteLine(string.Format("v {2} {1} {0}", -vertex.Item1, vertex.Item2, vertex.Item3));
                }

                writer.WriteLine();

                foreach (var normal in fumFile.normals)
                {
                    writer.WriteLine(string.Format("vn {2} {1} {0}", -normal.Item1, normal.Item2, normal.Item3));
                }

                writer.WriteLine();

                foreach (var uvcoord in fumFile.mapping)
                {
                    writer.WriteLine(string.Format("vt {0} {1}", uvcoord.Item1, uvcoord.Item2));
                }

                writer.WriteLine();

                foreach (var material in fumFile.materials)
                {
                    writer.WriteLine("g " + material.name);
                    writer.WriteLine("usemtl " + material.name);

                    foreach (var trinagle in material.triangles) // ZModeler plugin support (cause "f {0} {1} {2}" is enough for 3DS Max)
                    {
                        writer.WriteLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", trinagle.Item1 + 1, trinagle.Item2 + 1, trinagle.Item3 + 1));
                    }

                    writer.WriteLine();
                }
            }
        }
    }
}
