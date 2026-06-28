class IBKRError(RuntimeError):
    """Base error for the optional IBKR read-only boundary."""


class IBKRSafetyError(IBKRError):
    pass


class IBKRConnectionError(IBKRError):
    pass


class IBKRDependencyError(IBKRError):
    pass


class IBKRReadOnlyViolation(IBKRSafetyError):
    pass

