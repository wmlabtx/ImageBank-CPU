using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public static class NeuralGas
    {
        private const int MAXNEURONS = 5000;
        private const int LAMBDA = 5000;
        private const int MAXAXONAGE = 25;
        private const float EW = 0.0001f;
        private const float EA = 0.5f;

        private static readonly SortedList<int, Neuron> _neurons = new SortedList<int, Neuron>();
        private static readonly List<Axon> _axons = new List<Axon>();
        private static int _descriptorscounter = 0;

        public static void Clear()
        {
            _neurons.Clear();
            _axons.Clear();
            _descriptorscounter = 0;
        }

        public static void Init(RootSiftDescriptor[] descriptors)
        {
            var n1 = new Neuron(1, descriptors[0]);
            var n2 = new Neuron(2, descriptors[1]);
            _neurons.Add(n1.Id, n1);
            _neurons.Add(n2.Id, n2);
            var a = new Axon(1, 2);
            _axons.Add(a);
        }

        private static void FindNeurons(RootSiftDescriptor descriptor, out int v, out float vd, out int w, out float wd)
        {
            v = 0;
            vd = float.MaxValue;
            w = 0;
            wd = float.MaxValue;
            for (var i = 0; i < _neurons.Keys.Count; i++) {
                var distance = _neurons[_neurons.Keys[i]].GetDistance(descriptor);
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

        private static Axon FindAxon(int v, int w)
        {
            for (var i = 0; i < _axons.Count; i++) {
                if ((v == _axons[i].IdFrom && w == _axons[i].IdTo) || (v == _axons[i].IdTo && w == _axons[i].IdFrom)) {
                    return _axons[i];
                }
            }

            return null;
        }

        private static void DeleteOldAxons()
        {
            _axons.RemoveAll(e => e.Age > MAXAXONAGE);
        }

        private static void DeleteOldNeurons()
        {
            var dict = new SortedList<int, Neuron>(_neurons);
            foreach (var e in _axons) {
                if (dict.ContainsKey(e.IdFrom)) {
                    dict.Remove(e.IdFrom);
                }

                if (dict.ContainsKey(e.IdTo)) {
                    dict.Remove(e.IdTo);
                }
            }

            if (dict.Count == 0) {
                return;
            }

            foreach (var e in dict) {
                _neurons.Remove(e.Key);
                Debug.WriteLine($"delete neuron {e.Key}");
            }
        }

        private static void AgeAxons(int v)
        {
            for (var i = 0; i < _axons.Count; i++) {
                if (v == _axons[i].IdFrom || v == _axons[i].IdTo) {
                    _axons[i].IncrementAge();
                }
            }
        }

        private static int FindMaxErrorNeuron()
        {
            var u = 0;
            var maxerror = -1f;
            for (var i = 0; i < _neurons.Keys.Count; i++) {
                if (_neurons[_neurons.Keys[i]].Error > maxerror) {
                    u = _neurons.Keys[i];
                    maxerror = _neurons[u].Error;
                }
            }

            return u;
        }

        private static int FindMaxErrorNeuron(int u)
        {
            var v = 0;
            var maxerror = -1f;
            for (var i = 0; i < _axons.Count; i++) {
                int vx;
                if (u == _axons[i].IdFrom) {
                    vx = _axons[i].IdTo;
                }
                else {
                    if (u == _axons[i].IdTo) {
                        vx = _axons[i].IdFrom;
                    }
                    else {
                        continue;
                    }
                }

                if (_neurons[vx].Error > maxerror) {
                    v = vx;
                    maxerror = _neurons[vx].Error;
                }
            }

            return v;
        }

        public static int GetMaxId()
        {
            if (_neurons.Count == 0) {
                return 0;
            }

            var result = _neurons.Max(e => e.Value.Id);
            return result;
        }

        public static void LearnDescriptor(RootSiftDescriptor descriptor)
        {
            FindNeurons(descriptor, out int v, out float vd, out int w, out float wd);
            _neurons[v].MoveToward(descriptor, EW);
            _neurons[v].AddError(vd * vd);
            AgeAxons(v);
            var axon = FindAxon(v, w);
            if (axon == null) {
                var a = new Axon(v, w);
                _axons.Add(a);
            }
            else {
                axon.ResetAge();
            }

            DeleteOldAxons();
            DeleteOldNeurons();

            _descriptorscounter++;
            if (_neurons.Count >= MAXNEURONS || _descriptorscounter % LAMBDA != 0) {
                return;
            }

            var u = FindMaxErrorNeuron();
            v = FindMaxErrorNeuron(u);
            var r = GetMaxId() + 1;
            var error = _neurons[u].Error * EA;
            var newneuron = _neurons[u].Average(r, _neurons[v].GetVector());
            _neurons.Add(newneuron.Id, newneuron);
            _neurons[u].SetError(error);
            _neurons[r].SetError(error);
            error = _neurons[v].Error * EA;
            _neurons[v].SetError(error);
            axon = FindAxon(u, v);
            if (axon != null) {
                _axons.Remove(axon);
            }

            var a1 = new Axon(u, r);
            _axons.Add(a1);
            var a2 = new Axon(r, v);
            _axons.Add(a2);
        }

        public static void LearnDescriptors(RootSiftDescriptor[] descriptors)
        {
            for (var i = 0; i < descriptors.Length; i++) {
                LearnDescriptor(descriptors[i]);                
            }

            Debug.WriteLine($"neurons={_neurons.Count}, axons={_axons.Count}");
        }

        private static void FindNeuron(RootSiftDescriptor descriptor, out ushort id, out float error)
        {
            id = 0;
            error = float.MaxValue;
            for (var i = 0; i < _neurons.Keys.Count; i++) {
                var distance = _neurons[_neurons.Keys[i]].GetDistance(descriptor);
                if (distance < error) {
                    id = (ushort)_neurons.Keys[i];
                    error = distance;
                }
            }
        }

        public static void Compute(RootSiftDescriptor[] descriptors, out ushort[] vector, out float averageerror)
        {            
            vector = new ushort[descriptors.Length];
            averageerror = 0f;
            for (var i = 0; i < descriptors.Length; i++) {
                FindNeuron(descriptors[i], out ushort id, out float error);
                vector[i] = id;
                averageerror += error;
            }

            Array.Sort(vector);
            averageerror /= descriptors.Length;
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
                bw.Write(_descriptorscounter);
                bw.Write(_neurons.Keys.Count);
                for (var i = 0; i < _neurons.Keys.Count; i++) {
                    _neurons[_neurons.Keys[i]].Save(bw);
                }

                bw.Write(_axons.Count);
                for (var i = 0; i < _axons.Count; i++) {
                    _axons[i].Save(bw);
                }
            }
        }

        public static void Load()
        {
            Clear();
            using (var fs = new FileStream(AppConsts.FileNeuralGas, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs)) {
                _descriptorscounter = br.ReadInt32();
                var counter = br.ReadInt32();
                for (var i = 0; i < counter; i++) {
                    var neuron = new Neuron(br);
                    _neurons.Add(neuron.Id, neuron);
                }

                counter = br.ReadInt32();
                for (var i = 0; i < counter; i++) {
                    var axon = new Axon(br);
                    _axons.Add(axon);
                }
            }
        }
    }
}
