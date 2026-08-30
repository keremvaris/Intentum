from __future__ import annotations
from collections.abc import Callable
from dataclasses import dataclass, field
from kiota_abstractions.serialization import AdditionalDataHolder, Parsable, ParseNode, SerializationWriter
from typing import Any, Optional, TYPE_CHECKING, Union
from uuid import UUID

if TYPE_CHECKING:
    from .space_response_vector import SpaceResponse_vector

@dataclass
class SpaceResponse(AdditionalDataHolder, Parsable):
    # Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.
    additional_data: dict[str, Any] = field(default_factory=dict)

    # The eventCount property
    event_count: Optional[int] = None
    # The id property
    id: Optional[UUID] = None
    # The vector property
    vector: Optional[SpaceResponse_vector] = None
    
    @staticmethod
    def create_from_discriminator_value(parse_node: ParseNode) -> SpaceResponse:
        """
        Creates a new instance of the appropriate class based on discriminator value
        param parse_node: The parse node to use to read the discriminator value and create the object
        Returns: SpaceResponse
        """
        if parse_node is None:
            raise TypeError("parse_node cannot be null.")
        return SpaceResponse()
    
    def get_field_deserializers(self,) -> dict[str, Callable[[ParseNode], None]]:
        """
        The deserialization information for the current model
        Returns: dict[str, Callable[[ParseNode], None]]
        """
        from .space_response_vector import SpaceResponse_vector

        from .space_response_vector import SpaceResponse_vector

        fields: dict[str, Callable[[Any], None]] = {
            "eventCount": lambda n : setattr(self, 'event_count', n.get_int_value()),
            "id": lambda n : setattr(self, 'id', n.get_uuid_value()),
            "vector": lambda n : setattr(self, 'vector', n.get_object_value(SpaceResponse_vector)),
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
        writer.write_int_value("eventCount", self.event_count)
        writer.write_uuid_value("id", self.id)
        writer.write_object_value("vector", self.vector)
        writer.write_additional_data_value(self.additional_data)
    

