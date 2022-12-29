using System.Collections.Generic;
using Dbdeploy.Core.Scripts;

namespace Dbdeploy.Core
{
    public interface IChangeScriptApplier
    {
        void Apply(IEnumerable<ChangeScript> changeScripts, bool createChangeLogTable);
    }
}