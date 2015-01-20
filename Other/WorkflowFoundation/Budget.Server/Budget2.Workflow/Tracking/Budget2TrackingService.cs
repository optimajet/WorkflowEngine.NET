using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Tracking;

namespace Budget2.Workflow.Tracking
{
    public class Budget2TrackingService : TrackingService
    {
        private TrackingProfile _default;

        private volatile object _lock = new object();

        public TrackingProfile Default
        {
            get
            {
                if (_default == null)
                {
                    lock(_lock)
                    {
                        if (_default == null)
                        {
                            _default = new TrackingProfile();
                            _default.Version = new Version(1,0);
                            var trackingLocation = new ActivityTrackingLocation();
                            trackingLocation.ActivityType = typeof (StateActivity);
                            trackingLocation.ExecutionStatusEvents.Add(ActivityExecutionStatus.Initialized);
                            trackingLocation.ExecutionStatusEvents.Add(ActivityExecutionStatus.Executing);
                            var trackPoint = new ActivityTrackPoint();
                            trackPoint.MatchingLocations.Add(trackingLocation);
                            _default.ActivityTrackPoints.Add(trackPoint);
                        }
                    }
                }

                return _default;
            }

        }

        protected override TrackingChannel GetTrackingChannel(TrackingParameters parameters)
        {
            return new Budget2TrackingChannel(parameters);
        }

        protected override bool TryGetProfile(Type workflowType, out TrackingProfile profile)
        {
            try
            {
                profile = Default;
                return true;
            }
            catch (Exception)
            {
                profile = null;
                return false;
            }
            
        }

        protected override TrackingProfile GetProfile(Type workflowType, Version profileVersionId)
        {
            return Default;
        }

        protected override TrackingProfile GetProfile(Guid workflowInstanceId)
        {
            return Default;
        }

        protected override bool TryReloadProfile(Type workflowType, Guid workflowInstanceId, out TrackingProfile profile)
        {
            lock (_lock)
            {
                _default = null;
            }

            profile = _default;

            return true;
        }
    }
}
