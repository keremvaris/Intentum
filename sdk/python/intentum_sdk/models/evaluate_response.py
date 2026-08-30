from __future__ import annotations
from collections.abc import Callable
from dataclasses import dataclass, field
from kiota_abstractions.serialization import AdditionalDataHolder, Parsable, ParseNode, SerializationWriter
from typing import Any, Optional, TYPE_CHECKING, Union

if TYPE_CHECKING:
    from .evaluate_response_decision import EvaluateResponse_decision

@dataclass
class EvaluateResponse(AdditionalDataHolder, Parsable):
    # Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.
    additional_data: dict[str, Any] = field(default_factory=dict)

    # The decision property
    decision: Optional[EvaluateResponse_decision] = None
    
    @staticmethod
    def create_from_discriminator_value(parse_node: ParseNode) -> EvaluateResponse:
        """
        Creates a new instance of the appropriate class based on discriminator value
        param parse_node: The parse node to use to read the discriminator value and create the object
        Returns: EvaluateResponse
        """
        if parse_node is None:
            raise TypeError("parse_node cannot be null.")
        return EvaluateResponse()
    
    def get_field_deserializers(self,) -> dict[str, Callable[[ParseNode], None]]:
        """
        The deserialization information for the current model
        Returns: dict[str, Callable[[ParseNode], None]]
        """
        from .evaluate_response_decision import EvaluateResponse_decision

        from .evaluate_response_decision import EvaluateResponse_decision

        fields: dict[str, Callable[[Any], None]] = {
            "decision": lambda n : setattr(self, 'decision', n.get_enum_value(EvaluateResponse_decision)),
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
        writer.write_enum_value("decision", self.decision)
        writer.write_additional_data_value(self.additional_data)
    

