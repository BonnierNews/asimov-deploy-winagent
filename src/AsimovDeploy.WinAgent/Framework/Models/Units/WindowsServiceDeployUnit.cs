/*******************************************************************************
* Copyright (C) 2012 eBay Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
******************************************************************************/

using System.Linq;
using System.ServiceProcess;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Deployment.Steps;
using AsimovDeploy.WinAgent.Framework.Models.UnitActions;
using AsimovDeploy.WinAgent.Framework.Tasks;

namespace AsimovDeploy.WinAgent.Framework.Models.Units
{
    public class WindowsServiceDeployUnit : DeployUnit, ICanBeStopStarted
    {
        private string _serviceName; 
        public string ServiceName 
        {
            get { return _serviceName ?? Name;}
            set { _serviceName = value; } 
        }

        public string Url { get; set; }

	    public WindowsServiceDeployUnit()
	    {
			Actions.Add(new StartDeployUnitAction() { Sort = 10 });
			Actions.Add(new StopDeployUnitAction() { Sort = 11 });
	    }

        public override AsimovTask GetDeployTask(AsimovVersion version, ParameterValues parameterValues, AsimovUser user)
        {
            var task = new DeployTask(this, version, parameterValues, user);
            task.AddDeployStep<UpdateWindowsService>();
            foreach (var action in Actions.OfType<CommandUnitAction>())
            {
                task.AddDeployStep(new ExecuteUnitAction(action, user));
            }
            return task;
        }

        public override DeployUnitInfo GetUnitInfo()
        {
            var serviceManager = new ServiceController(ServiceName);

            var unitInfo = base.GetUnitInfo();
			if (!string.IsNullOrEmpty(Url))
			{
				unitInfo.Url = Url.Replace("localhost", HostNameUtil.GetFullHostName());
			}

            try
            {
                unitInfo.Status = serviceManager.Status == ServiceControllerStatus.Running ? UnitStatus.Running : UnitStatus.Stopped;
            }
            catch
            {
                unitInfo.Status = UnitStatus.NotFound;
            }

            return unitInfo;
        }

	    public AsimovTask GetStopTask()
	    {
		    return new StartStopWindowsServiceTask(this, stop: true);
	    }

	    public AsimovTask GetStartTask()
	    {
			return new StartStopWindowsServiceTask(this, stop: false);
	    }
    }
}