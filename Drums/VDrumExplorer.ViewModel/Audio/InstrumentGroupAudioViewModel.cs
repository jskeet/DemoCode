using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Audio;

namespace VDrumExplorer.ViewModel.Audio
{
    public class InstrumentGroupAudioViewModel
    {
        public InstrumentGroup Group { get; }
        public IReadOnlyList<InstrumentAudio> Audio { get; }

        public InstrumentGroupAudioViewModel(InstrumentGroup group, IReadOnlyList<InstrumentAudio> audio) =>
            (Group, Audio) = (group, audio);

        public static InstrumentGroupAudioViewModel FromGrouping(IGrouping<InstrumentGroup, InstrumentAudio> grouping) =>
            new InstrumentGroupAudioViewModel(grouping.Key, grouping.ToList());
    }
}
