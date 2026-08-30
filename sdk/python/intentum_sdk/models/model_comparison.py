from __future__ import annotations
from collections.abc import Callable
from dataclasses import dataclass, field
from kiota_abstractions.serialization import AdditionalDataHolder, Parsable, ParseNode, SerializationWriter
from typing import Any, Optional, TYPE_CHECKING, Union

if TYPE_CHECKING:
    from .intent import Intent

@dataclass
class ModelComparison(AdditionalDataHolder, Parsable):
    # Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.
    additional_data: dict[str, Any] = field(default_factory=dict)

    # The elapsedMs property
    elapsed_ms: Optional[float] = None
    # The intent property
    intent: Optional[Intent] = None
    # The modelName property
    model_name: Optional[str] = None
    
    @staticmethod
    def create_from_discriminator_value(parse_node: ParseNode) -> ModelComparison:
        """
        Creates a new instance of the appropriate class based on discriminator value
        param parse_node: The parse node to use to read the discriminator value and create the object
        Returns: ModelComparison
        """
        if parse_node is None:
            raise TypeError("parse_node cannot be null.")
        return ModelComparison()
    
    def get_field_deserializers(self,) -> dict[str, Callable[[ParseNode], None]]:
        """
        The deserialization information for the current model
        Returns: dict[str, Callable[[ParseNode], None]]
        """
        from .intent import Intent

        from .intent import Intent

        fields: dict[str, Callable[[Any], None]] = {
            "elapsedMs": lambda n : setattr(self, 'elapsed_ms', n.get_float_value()),
            "intent": lambda n : setattr(self, 'intent', n.get_object_value(Intent)),
            "modelName": lambda n : setattr(self, 'model_name', n.get_str_value()),
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
        writer.write_float_value("elapsedMs", self.elapsed_ms)
        writer.write_object_value("intent", self.intent)
        writer.write_str_value("modelName", self.model_name)
        writer.write_additional_data_value(self.additional_data)
    

