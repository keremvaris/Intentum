from __future__ import annotations
from collections.abc import Callable
from dataclasses import dataclass, field
from kiota_abstractions.serialization import AdditionalDataHolder, Parsable, ParseNode, SerializationWriter
from typing import Any, Optional, TYPE_CHECKING, Union

if TYPE_CHECKING:
    from .timeline_entry import TimelineEntry

@dataclass
class TimelineResponse(AdditionalDataHolder, Parsable):
    # Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.
    additional_data: dict[str, Any] = field(default_factory=dict)

    # The entityId property
    entity_id: Optional[str] = None
    # The entries property
    entries: Optional[list[TimelineEntry]] = None
    
    @staticmethod
    def create_from_discriminator_value(parse_node: ParseNode) -> TimelineResponse:
        """
        Creates a new instance of the appropriate class based on discriminator value
        param parse_node: The parse node to use to read the discriminator value and create the object
        Returns: TimelineResponse
        """
        if parse_node is None:
            raise TypeError("parse_node cannot be null.")
        return TimelineResponse()
    
    def get_field_deserializers(self,) -> dict[str, Callable[[ParseNode], None]]:
        """
        The deserialization information for the current model
        Returns: dict[str, Callable[[ParseNode], None]]
        """
        from .timeline_entry import TimelineEntry

        from .timeline_entry import TimelineEntry

        fields: dict[str, Callable[[Any], None]] = {
            "entityId": lambda n : setattr(self, 'entity_id', n.get_str_value()),
            "entries": lambda n : setattr(self, 'entries', n.get_collection_of_object_values(TimelineEntry)),
        }
        return fields
    
    def serialize(self,writer: SerializationWriter) -> None:
        """
        Serializes information the current object
        param writer: Serialization writer to use to serialize this model
        Returns: None
        """
        if writer is None:
            raise TypeError("writer cannot be null.")
        writer.write_str_value("entityId", self.entity_id)
        writer.write_collection_of_object_values("entries", self.entries)
        writer.write_additional_data_value(self.additional_data)
    

