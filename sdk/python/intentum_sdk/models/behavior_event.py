from __future__ import annotations
import datetime
from collections.abc import Callable
from dataclasses import dataclass, field
from kiota_abstractions.serialization import AdditionalDataHolder, Parsable, ParseNode, SerializationWriter
from typing import Any, Optional, TYPE_CHECKING, Union

if TYPE_CHECKING:
    from .behavior_event_metadata import BehaviorEvent_metadata

@dataclass
class BehaviorEvent(AdditionalDataHolder, Parsable):
    # Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.
    additional_data: dict[str, Any] = field(default_factory=dict)

    # Event action
    action: Optional[str] = None
    # Event actor
    actor: Optional[str] = None
    # Additional metadata
    metadata: Optional[BehaviorEvent_metadata] = None
    # Event timestamp
    timestamp: Optional[datetime.datetime] = None
    
    @staticmethod
    def create_from_discriminator_value(parse_node: ParseNode) -> BehaviorEvent:
        """
        Creates a new instance of the appropriate class based on discriminator value
        param parse_node: The parse node to use to read the discriminator value and create the object
        Returns: BehaviorEvent
        """
        if parse_node is None:
            raise TypeError("parse_node cannot be null.")
        return BehaviorEvent()
    
    def get_field_deserializers(self,) -> dict[str, Callable[[ParseNode], None]]:
        """
        The deserialization information for the current model
        Returns: dict[str, Callable[[ParseNode], None]]
        """
        from .behavior_event_metadata import BehaviorEvent_metadata

        from .behavior_event_metadata import BehaviorEvent_metadata

        fields: dict[str, Callable[[Any], None]] = {
            "action": lambda n : setattr(self, 'action', n.get_str_value()),
            "actor": lambda n : setattr(self, 'actor', n.get_str_value()),
            "metadata": lambda n : setattr(self, 'metadata', n.get_object_value(BehaviorEvent_metadata)),
            "timestamp": lambda n : setattr(self, 'timestamp', n.get_datetime_value()),
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
        writer.write_str_value("action", self.action)
        writer.write_str_value("actor", self.actor)
        writer.write_object_value("metadata", self.metadata)
        writer.write_datetime_value("timestamp", self.timestamp)
        writer.write_additional_data_value(self.additional_data)
    

