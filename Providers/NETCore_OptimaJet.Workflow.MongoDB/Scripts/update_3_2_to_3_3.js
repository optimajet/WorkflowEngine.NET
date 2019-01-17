db.WorkflowProcessInstance.find({ Status: { $exists: false } }).forEach(
    function (elem) {
        
        var status = db.WorkflowProcessInstanceStatus.findOne({ _id: elem._id });
              
        db.WorkflowProcessInstance.update(
            {
                _id: elem._id
            },
            {
                $set: {
                    Status: {
                        Lock: status.Lock,
                        Status: status.Status
                    }
                }
            }
        );
    }
);
    
db.WorkflowProcessInstanceStatus.drop();