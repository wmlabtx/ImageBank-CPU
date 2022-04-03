using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public static class NeuralGas
    {
        private const ushort Maxneurons = 4 * 1024;
        private const int Maxaxonage = 256;
        private const float Ew = 0.01f;
        private const float Ex = 0.001f;
        private const float Ea = 0.5f;

        private static readonly SortedList<ushort, Neuron> _neurons = new SortedList<ushort, Neuron>();
        private static readonly List<Axon> _axons = new List<Axon>();

        public static void Clear()
        {
            _neurons.Clear();
            _axons.Clear();
        }

        private static void Init(RootSiftDescriptor[] descriptors)
        {
            var n1 = new Neuron(1, descriptors[0]);
            var n2 = new Neuron(2, descriptors[1]);
            _neurons.Add(n1.Id, n1);
            _neurons.Add(n2.Id, n2);
            ResetAxon(1, 2);
        }

        private static void ResetAxon(ushort x, ushort y)
        {
            var a = _axons.FirstOrDefault(e => e.X == x && e.Y == y || e.X == y && e.Y == x);
            if (a == null) {
                _axons.Add(new Axon(x, y));
            }
            else {
                 a.Age = 0;
            }

            var removed = false;
            var list = _axons.Where(e => e.X == x || e.Y == x).ToArray();
            foreach (var e in list) {
                if (e.X == x && e.Y == y || e.X == y && e.Y == x) {
                    continue;
                }

                e.Age++;
                if (e.Age > Maxaxonage) {
                    _axons.Remove(e);
                    removed = true;
                }
            }

            if (removed) {
                DeleteIsolatedNeurons();
            }
        }

        private static void DeleteIsolatedNeurons()
        {
            var dict = new SortedList<ushort, Neuron>(_neurons);
            foreach (var e in _axons) {
                if (dict.ContainsKey(e.X)) {
                    dict.Remove(e.X);
                }

                if (dict.ContainsKey(e.Y)) {
                    dict.Remove(e.Y);
                }
            }

            if (dict.Count == 0) {
                return;
            }

            foreach (var e in dict) {
                _neurons.Remove(e.Key);
            }
        }

        private static void FindTwoNeurons(RootSiftDescriptor descriptor, out ushort v, out float vd, out ushort w)
        {
            v = 0;
            vd = 128f;
            w = 0;
            var wd = 128f;
            var neuronsarray = _neurons.ToArray();
            for (var i = 0; i < neuronsarray.Length; i++) {
                var distance = neuronsarray[i].Value.GetDistance(descriptor);
                if (distance < vd) {
                    w = v;
                    wd = vd;
                    v = _neurons.Keys[i];
                    vd = distance;
                }
                else {
                    if (distance < wd) {
                        w = _neurons.Keys[i];
                        wd = distance;
                    }
                }
            }
        }

        private static IEnumerable<ushort> GetNeighbors(ushort x)
        {
            var list = new List<ushort>();
            var axonsarray = _axons.ToArray();
            foreach (var e in axonsarray) {
                if (e.X == x) {
                    list.Add(e.Y);
                }
                else {
                    if (e.Y == x) {
                        list.Add(e.X);
                    }
                }
            }

            return list.ToArray();
        }

        private static ushort GetAvailableNeuronId()
        {
            ushort id = 1;
            while (id <= Maxneurons) {
                if (!_neurons.ContainsKey(id)) {
                    return id;
                }

                id++;
            }

            return 0;
        }

        private static ushort FindMaxErrorNeuron()
        {
            ushort u = 0;
            var maxerror = 0f;
            var neuronsarray = _neurons.ToArray();
            foreach (var t in neuronsarray) {
                if (t.Value.Error > maxerror) {
                    u = t.Key;
                    maxerror = t.Value.Error;
                }
            }

            return u;
        }

        private static ushort FindMaxErrorNeuron(ushort u)
        {
            ushort v = 0;
            var maxerror = -1f;
            var neighbors = GetNeighbors(u);
            foreach (var n in neighbors) {
                if (_neurons[n].Error > maxerror) {
                    v = n;
                    maxerror = _neurons[n].Error;
                }
            }

            return v;
        }

        private static void SplitNeuron()
        {
            if (_neurons.Count >= Maxneurons) {
                return;
            }

            var u = FindMaxErrorNeuron();
            var v = FindMaxErrorNeuron(u);
            var r = GetAvailableNeuronId();
            var error = _neurons[u].Error * Ea;
            var newneuron = _neurons[u].Average(r, _neurons[v].GetVector());
            _neurons.Add(newneuron.Id, newneuron);
            _neurons[u].Error = error;
            _neurons[r].Error = error;
            error = _neurons[v].Error * Ea;
            _neurons[v].Error = error;
            var a = _axons.FirstOrDefault(e => e.X == u && e.Y == v || e.X == v && e.Y == u);
            if (a != null) {
                _axons.Remove(a);
            }

            var a1 = new Axon(u, r);
            _axons.Add(a1);
            var a2 = new Axon(r, v);
            _axons.Add(a2);
        }

        private static void LearnDescriptor(RootSiftDescriptor descriptor)
        {
            FindTwoNeurons(descriptor, out var v, out var vd, out var w);
            _neurons[v].MoveToward(descriptor, Ew);
            _neurons[v].Error += vd * vd;
            var neighbors = GetNeighbors(v);
            foreach (var n in neighbors) {
                _neurons[n].MoveToward(descriptor, Ex);
            }

            ResetAxon(v, w);
        }

        public static void LearnDescriptors(RootSiftDescriptor[] descriptors)
        {
            if (_neurons.Count == 0) {
                Init(descriptors);
            }

            foreach (var t in descriptors) {
                LearnDescriptor(t);
            }

            SplitNeuron();
            //Debug.WriteLine($"neurons={_neurons.Count}, axons={_axons.Count}");
        }

        private static void FindNeuron(RootSiftDescriptor descriptor, out ushort id, out float error)
        {
            id = 0;
            error = 128f;
            var neuronsarray = _neurons.ToArray();
            foreach (var t in neuronsarray) {
                var distance = t.Value.GetDistance(descriptor);
                if (distance < error) {
                    id = t.Key;
                    error = distance;
                }
            }
        }

        public static void Compute(RootSiftDescriptor[] descriptors, out ushort[] vector, out float minerror, out float maxerror)
        {            
            vector = new ushort[descriptors.Length];
            minerror = 128f;
            maxerror = 0f;
            for (var i = 0; i < descriptors.Length; i++) {
                FindNeuron(descriptors[i], out var id, out var error);
                vector[i] = id;
                minerror = Math.Min(minerror, error);
                maxerror = Math.Max(maxerror, error);
            }

            Array.Sort(vector);
        }

        public static void Save()
        {
            if (File.Exists(AppConsts.FileNeuralGas)) {
                var bak = $"{AppConsts.FileNeuralGas}.bak";
                if (File.Exists(bak)) {
                    File.Delete(bak);
                }

                File.Move(AppConsts.FileNeuralGas, bak);
            }

            using (var fs = new FileStream(AppConsts.FileNeuralGas, FileMode.CreateNew, FileAccess.Write))
            using (var bw = new BinaryWriter(fs)) {
                var neuronsarray = _neurons.ToArray();
                bw.Write(neuronsarray.Length);
                foreach (var t in neuronsarray) {
                    t.Value.Save(bw);
                }

                var axonsarray = _axons.ToArray();
                bw.Write(axonsarray.Length);
                foreach (var t in axonsarray) {
                    t.Save(bw);
                }
            }
        }

        public static void Load()
        {
            Clear();
            using (var fs = new FileStream(AppConsts.FileNeuralGas, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs)) {
                var neuronscounter = br.ReadInt32();
                for (var i = 0; i < neuronscounter; i++) {
                    var neuron = new Neuron(br);
                    _neurons.Add(neuron.Id, neuron);
                }

                var axonscounter = br.ReadInt32();
                for (var i = 0; i < axonscounter; i++) {
                    var axon = new Axon(br);
                    _axons.Add(axon);
                }
            }
        }

        public static void GetStats(out int neurons, out int axons)
        {
            neurons = _neurons.Count;
            axons = _axons.Count;
        }
    }
}
