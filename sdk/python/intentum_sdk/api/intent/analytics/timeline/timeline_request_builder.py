from __future__ import annotations
from collections.abc import Callable
from kiota_abstractions.base_request_builder import BaseRequestBuilder
from kiota_abstractions.get_path_parameters import get_path_parameters
from kiota_abstractions.request_adapter import RequestAdapter
from typing import Any, Optional, TYPE_CHECKING, Union

if TYPE_CHECKING:
    from .item.with_entity_item_request_builder import WithEntityItemRequestBuilder

class TimelineRequestBuilder(BaseRequestBuilder):
    """
    Builds and executes requests for operations under /api/intent/analytics/timeline
    """
    def __init__(self,request_adapter: RequestAdapter, path_parameters: Union[str, dict[str, Any]]) -> None:
        """
        Instantiates a new TimelineRequestBuilder and sets the default values.
        param path_parameters: The raw url or the url-template parameters for the request.
        param request_adapter: The request adapter to use to execute the requests.
        Returns: None
        """
        super().__init__(request_adapter, "{+baseurl}/api/intent/analytics/timeline", path_parameters)
    
    def by_entity_id(self,entity_id: str) -> WithEntityItemRequestBuilder:
        """
        Gets an item from the ApiSdk.api.intent.analytics.timeline.item collection
        param entity_id: Unique identifier of the item
        Returns: WithEntityItemRequestBuilder
        """
        if entity_id is None:
            raise TypeError("entity_id cannot be null.")
        from .item.with_entity_item_request_builder import WithEntityItemRequestBuilder

        url_tpl_params = get_path_parameters(self.path_parameters)
        url_tpl_params["entityId"] = entity_id
        return WithEntityItemRequestBuilder(self.request_adapter, url_tpl_params)
    

