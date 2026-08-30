from __future__ import annotations
import datetime
from collections.abc import Callable
from dataclasses import dataclass, field
from kiota_abstractions.serialization import AdditionalDataHolder, Parsable, ParseNode, SerializationWriter
from typing import Any, Optional, TYPE_CHECKING, Union

if TYPE_CHECKING:
    from .confidence import Confidence

@dataclass
class TimelineEntry(AdditionalDataHolder, Parsable):
    # Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.
    additional_data: dict[str, Any] = field(default_factory=dict)

    # The confidence property
    confidence: Optional[Confidence] = None
    # The decision property
    decision: Optional[str] = None
    # The intentName property
    intent_name: Optional[str] = None
    # The timestamp property
    timestamp: Optional[datetime.datetime] = None
    
    @staticmethod
    def create_from_discriminator_value(parse_node: ParseNode) -> TimelineEntry:
        """
        Creates a new instance of the appropriate class based on discriminator value
        param parse_node: The parse node to use to read the discriminator value and create the object
        Returns: TimelineEntry
        """
        if parse_node is None:
            raise TypeError("parse_node cannot be null.")
        return TimelineEntry()
    
    def get_field_deserializers(self,) -> dict[str, Callable[[ParseNode], None]]:
        """
        The deserialization information for the current model
        Returns: dict[str, Callable[[ParseNode], None]]
        """
        from .confidence import Confidence

        from .confidence import Confidence

        fields: dict[str, Callable[[Any], None]] = {
            "confidence": lambda n : setattr(self, 'confidence', n.get_object_value(Confidence)),
            "decision": lambda n : setattr(self, 'decision', n.get_str_value()),
            "intentName": lambda n : setattr(self, 'intent_name', n.get_str_value()),
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
        writer.write_object_value("confidence", self.confidence)
        writer.write_str_value("decision", self.decision)
        writer.write_str_value("intentName", self.intent_name)
        writer.write_datetime_value("timestamp", self.timestamp)
        writer.write_additional_data_value(self.additional_data)
    

