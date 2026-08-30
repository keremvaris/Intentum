from __future__ import annotations
from collections.abc import Callable
from kiota_abstractions.base_request_builder import BaseRequestBuilder
from kiota_abstractions.get_path_parameters import get_path_parameters
from kiota_abstractions.request_adapter import RequestAdapter
from typing import Any, Optional, TYPE_CHECKING, Union

if TYPE_CHECKING:
    from .analytics.analytics_request_builder import AnalyticsRequestBuilder
    from .evaluate.evaluate_request_builder import EvaluateRequestBuilder
    from .explain_tree.explain_tree_request_builder import ExplainTreeRequestBuilder
    from .infer.infer_request_builder import InferRequestBuilder
    from .playground.playground_request_builder import PlaygroundRequestBuilder

class IntentRequestBuilder(BaseRequestBuilder):
    """
    Builds and executes requests for operations under /api/intent
    """
    def __init__(self,request_adapter: RequestAdapter, path_parameters: Union[str, dict[str, Any]]) -> None:
        """
        Instantiates a new IntentRequestBuilder and sets the default values.
        param path_parameters: The raw url or the url-template parameters for the request.
        param request_adapter: The request adapter to use to execute the requests.
        Returns: None
        """
        super().__init__(request_adapter, "{+baseurl}/api/intent", path_parameters)
    
    @property
    def analytics(self) -> AnalyticsRequestBuilder:
        """
        The analytics property
        """
        from .analytics.analytics_request_builder import AnalyticsRequestBuilder

        return AnalyticsRequestBuilder(self.request_adapter, self.path_parameters)
    
    @property
    def evaluate(self) -> EvaluateRequestBuilder:
        """
        The evaluate property
        """
        from .evaluate.evaluate_request_builder import EvaluateRequestBuilder

        return EvaluateRequestBuilder(self.request_adapter, self.path_parameters)
    
    @property
    def explain_tree(self) -> ExplainTreeRequestBuilder:
        """
        The explainTree property
        """
        from .explain_tree.explain_tree_request_builder import ExplainTreeRequestBuilder

        return ExplainTreeRequestBuilder(self.request_adapter, self.path_parameters)
    
    @property
    def infer(self) -> InferRequestBuilder:
        """
        The infer property
        """
        from .infer.infer_request_builder import InferRequestBuilder

        return InferRequestBuilder(self.request_adapter, self.path_parameters)
    
    @property
    def playground(self) -> PlaygroundRequestBuilder:
        """
        The playground property
        """
        from .playground.playground_request_builder import PlaygroundRequestBuilder

        return PlaygroundRequestBuilder(self.request_adapter, self.path_parameters)
    

