using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharkhDande.Core;

public interface ICustomSerializable
{
    string Serialize(WorkflowContext context);
}
