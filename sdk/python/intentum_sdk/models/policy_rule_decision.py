from enum import Enum

class PolicyRule_decision(str, Enum):
    Allow = "Allow",
    Observe = "Observe",
    Warn = "Warn",
    Block = "Block",
    Escalate = "Escalate",
    RequireAuth = "RequireAuth",
    RateLimit = "RateLimit",

