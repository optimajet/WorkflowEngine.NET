db.WorkflowProcessInstance.find({ RootProcessId: { $exists: false } }).forEach(
    function (elem) {
        db.WorkflowProcessInstance.update(
            {
                _id: elem._id
            },
            {
                $set: {
                    RootProcessId: elem._id
                }
            }
        );
    }
);
