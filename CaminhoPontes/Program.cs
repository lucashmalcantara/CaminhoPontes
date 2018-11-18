using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaminhoPontes
{
    class Program
    {
        private const string INPUT_TESTES_1 = "2 5\n0 1 1\n0 2 3\n0 3 9\n1 3 2\n2 3 2";
        private const string INPUT_TESTES_2 = "4 9\n0 1 1\n0 3 4\n0 4 2\n1 2 5\n1 5 3\n2 5 5\n3 4 2\n3 5 5\n4 5 8";

        private const char SEPARADOR_PADRAO = ' ';
        private const int INDICE_VERTICE_ORIGEM = 0;

        private class Ponte
        {
            public int PilarS { get; set; }
            public int PilarT { get; set; }
            public int QuantidadeBuracos { get; set; }

            public Ponte(string[] dadosPonte)
            {
                PilarS = int.Parse(dadosPonte[0]);
                PilarT = int.Parse(dadosPonte[1]);
                QuantidadeBuracos = int.Parse(dadosPonte[2]);
            }
        }

        static void Main(string[] args)
        {
            int quantidadePilares = 0;

            // Comentar para testar.
            var pontes = RetornarPontes(out quantidadePilares);

            // Descomentar para testar.
            //var pontes = RetornarPontesTeste(out quantidadePilares);

            var grafo = new Grafo(false);
            InicializarVertices(ref grafo, quantidadePilares);

            for (int i = 0; i < pontes.Count; i++)
                grafo.InserirAresta(pontes[i].PilarS, pontes[i].PilarT, pontes[i].QuantidadeBuracos);

            var indiceVerticeDestino = quantidadePilares + 1;

            var verticeOrigem = grafo.RetornarVertice(INDICE_VERTICE_ORIGEM);
            Dijkstra.Executar(grafo, verticeOrigem);


            var verticeDestino = grafo.RetornarVertice(indiceVerticeDestino);

            Console.Write(verticeDestino.Distancia);
        }

        private static List<Ponte> RetornarPontes(out int quantidadePilares)
        {
            var pontes = new List<Ponte>();

            var primeiraLinha = Console.ReadLine().Split(SEPARADOR_PADRAO);

            quantidadePilares = int.Parse(primeiraLinha[0]);
            var quantidadePontes = int.Parse(primeiraLinha[1]);

            for (int i = 0; i < quantidadePontes; i++)
            {
                var dadosPonte = Console.ReadLine().Split(SEPARADOR_PADRAO);
                var ponte = new Ponte(dadosPonte);
                pontes.Add(ponte);
            }

            return pontes;
        }

        private static List<Ponte> RetornarPontesTeste(out int quantidadePilares)
        {
            var linhasTeste = INPUT_TESTES_1.Split('\n');

            var pontes = new List<Ponte>();

            var primeiraLinha = linhasTeste[0].Split(SEPARADOR_PADRAO);

            quantidadePilares = int.Parse(primeiraLinha[0]);
            var quantidadePontes = int.Parse(primeiraLinha[1]);

            for (int i = 1; i <= quantidadePontes; i++)
            {
                var dadosPonte = linhasTeste[i].Split(SEPARADOR_PADRAO);
                var ponte = new Ponte(dadosPonte);
                pontes.Add(ponte);
            }

            return pontes;
        }

        private static void InicializarVertices(ref Grafo grafo, int quantidadePilares)
        {
            var totalVertices = quantidadePilares + 2;

            for (int i = 0; i < totalVertices; i++)
                grafo.InserirVertice(i);
        }

        #region Implementação Grafo
        class Grafo
        {
            public SortedDictionary<int, Vertice> Vertices { get; set; }
            private bool _direcionado = false;

            public Grafo(bool direcionado)
            {
                _direcionado = direcionado;
                Vertices = new SortedDictionary<int, Vertice>();
            }

            public void InserirVertice(int id)
            {
                var vertice = new Vertice(id);
                Vertices[id] = vertice;
            }

            public void InserirAresta(int de, int para, int peso)
            {
                var p = Vertices[para];
                var d = Vertices[de];

                Vertices[de].InserirVerticeAdjacente(p, peso);

                if (!_direcionado)
                    Vertices[para].InserirVerticeAdjacente(d, peso);
            }

            public List<Tuple<Vertice, Vertice>> RetornarArestas()
            {
                var arestas = new List<Tuple<Vertice, Vertice>>();

                foreach (KeyValuePair<int, Vertice> u in Vertices)
                {
                    foreach (KeyValuePair<Vertice, int> v in u.Value.RetornarAdjacentes())
                        arestas.Add(Tuple.Create(u.Value, v.Key));
                }

                return arestas;
            }

            public Vertice RetornarVertice(int vertice)
            {
                if (Vertices.ContainsKey(vertice))
                    return Vertices[vertice];

                return null;
            }
        }

        class Vertice
        {
            public int Id { get; set; }
            public int Distancia { get; set; }
            public bool Visitado { get; set; }
            public Vertice Anterior { get; set; }
            private Dictionary<Vertice, int> _adjacencias = new Dictionary<Vertice, int>();

            public Vertice(int id)
            {
                Id = id;
                Distancia = 0;
                Visitado = false;
                Anterior = null;
            }

            public void InserirVerticeAdjacente(Vertice para, int peso)
            {
                _adjacencias[para] = peso;
            }

            public Dictionary<Vertice, int> RetornarAdjacentes()
            {
                return _adjacencias.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }

            public int RetornarPeso(Vertice v)
            {
                return _adjacencias[v];
            }
        }

        class Dijkstra
        {
            public static void Executar(Grafo grafo, Vertice origem)
            {
                var Q = new SortedDictionary<int, Vertice>();

                Inicializar(ref grafo, ref origem);

                Q = grafo.Vertices;

                var S = new SortedDictionary<int, Vertice>();

                while (Q.Count > 0)
                {
                    Vertice u = ExtrairMinimo(ref Q);

                    u.Visitado = true;

                    foreach (KeyValuePair<Vertice, int> v in u.RetornarAdjacentes())
                    {
                        if (v.Key.Visitado) continue;

                        Relaxar(u, v.Key);
                    }

                    Adicionar(u, ref S);
                }

                grafo.Vertices = S;
            }

            private static void Inicializar(ref Grafo grafo, ref Vertice origem)
            {
                foreach (KeyValuePair<int, Vertice> v in grafo.Vertices)
                    v.Value.Distancia = int.MaxValue;

                origem.Distancia = 0;
            }

            private static Vertice ExtrairMinimo(ref SortedDictionary<int, Vertice> Q)
            {
                var key = Q.Keys.ToList()[0];
                Vertice minimo = Q[key];

                foreach (KeyValuePair<int, Vertice> v in Q)
                {
                    if (v.Value.Distancia < minimo.Distancia)
                        minimo = v.Value;
                }

                Q.Remove(minimo.Id);
                return minimo;
            }

            private static void Relaxar(Vertice u, Vertice v)
            {
                int distancia = u.Distancia + u.RetornarPeso(v);

                if (v.Distancia > distancia)
                {
                    v.Distancia = distancia;
                    v.Anterior = u;
                }
            }

            private static void Adicionar(Vertice u, ref SortedDictionary<int, Vertice> S)
            {
                Vertice vertice;

                if (S.TryGetValue(u.Id, out vertice))
                    vertice = u;
                else
                    S.Add(u.Id, u);
            }
        }
        #endregion
    }
}
