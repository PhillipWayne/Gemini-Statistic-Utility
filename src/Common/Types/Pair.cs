/*************************************************************************************
 *  Gemini Statistic Utility                                                         *
 *-----------------------------------------------------------------------------------*
 *  Copyright (c) 2016, Peter Baumann                                                *
 *  All rights reserved.                                                             *
 *                                                                                   *
 *  Redistribution and use in source and binary forms, with or without               *
 *  modification, are permitted provided that the following conditions are met:      *
 *    1. Redistributions of source code must retain the above copyright              *
 *       notice, this list of conditions and the following disclaimer.               *
 *    2. Redistributions in binary form must reproduce the above copyright           *
 *       notice, this list of conditions and the following disclaimer in the         *
 *       documentation and/or other materials provided with the distribution.        *
 *    3. Neither the name of the organization nor the                                *
 *       names of its contributors may be used to endorse or promote products        *
 *       derived from this software without specific prior written permission.       *
 *                                                                                   *
 *  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND  *
 *  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED    *
 *  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE           *
 *  DISCLAIMED. IN NO EVENT SHALL PETER BAUMANN BE LIABLE FOR ANY                    *
 *  DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES       *
 *  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;     *
 *  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND      *
 *  ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT       *
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS    *
 *  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.                     *
 *                                                                                   *
 *************************************************************************************/

using Microsoft.Practices.Prism.Mvvm;

namespace GeminiStatisticUtility.Common.Types {

    public class Pair<T> {
        public T First { get; set; }
        public T Second { get; set; }

        public Pair() { }
        public Pair(T first, T second) { this.First = first; this.Second = second; }
    };


    public class Pair<T, U> {
        public T First { get; set; }
        public U Second { get; set; }

        public Pair() { }
        public Pair(T first, U second) { this.First = first; this.Second = second; }
    };


    // Special MVVM Pair with responsive Property binding
    public class PairMVVM<T> : BindableBase {
        private T _first;
        private T _second;
        public T First { get { return _first; } set { SetProperty(ref _first, value); } }
        public T Second { get { return _second; } set { SetProperty(ref _second, value); } }

        public PairMVVM() { }
        public PairMVVM(T first, T second) { this.First = first; this.Second = second; }
    };


}
